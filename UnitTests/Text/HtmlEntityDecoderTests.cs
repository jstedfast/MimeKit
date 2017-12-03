//
// HtmlEntityDecoderTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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

namespace UnitTests.Text {
	[TestFixture]
	public class HtmlEntityDecoderTests
	{
		[Test]
		public void TestDecodeNamedEntities ()
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

		static void TestDecodeNumericEntity (string text, string expected)
		{
			var decoder = new HtmlEntityDecoder ();

			for (int i = 0; i < text.Length; i++)
				Assert.IsTrue (decoder.Push (text[i]), "Failed to push char #{0} of \"{1}\".", i, text);

			Assert.AreEqual (expected, decoder.GetValue (), "Decoded entity did not match for \"{0}\".", text);
		}

		[Test]
		public void TestDecodeNumericEntities ()
		{
			TestDecodeNumericEntity ("&#x00", "\uFFFD"); // REPLACEMENT CHARACTER
			TestDecodeNumericEntity ("&#x80", "\u20AC"); // EURO SIGN (€)
			TestDecodeNumericEntity ("&#x82", "\u201A"); // SINGLE LOW-9 QUOTATION MARK (‚)
			TestDecodeNumericEntity ("&#x83", "\u0192"); // LATIN SMALL LETTER F WITH HOOK (ƒ)
			TestDecodeNumericEntity ("&#x84", "\u201E"); // DOUBLE LOW-9 QUOTATION MARK („)
			TestDecodeNumericEntity ("&#x85", "\u2026"); // HORIZONTAL ELLIPSIS (…)
			TestDecodeNumericEntity ("&#x86", "\u2020"); // DAGGER (†)
			TestDecodeNumericEntity ("&#x87", "\u2021"); // DOUBLE DAGGER (‡)
			TestDecodeNumericEntity ("&#x88", "\u02C6"); // MODIFIER LETTER CIRCUMFLEX ACCENT (ˆ)
			TestDecodeNumericEntity ("&#x89", "\u2030"); // PER MILLE SIGN (‰)
			TestDecodeNumericEntity ("&#x8A", "\u0160"); // LATIN CAPITAL LETTER S WITH CARON (Š)
			TestDecodeNumericEntity ("&#x8B", "\u2039"); // SINGLE LEFT-POINTING ANGLE QUOTATION MARK (‹)
			TestDecodeNumericEntity ("&#x8C", "\u0152"); // LATIN CAPITAL LIGATURE OE (Œ)
			TestDecodeNumericEntity ("&#x8E", "\u017D"); // LATIN CAPITAL LETTER Z WITH CARON (Ž)
			TestDecodeNumericEntity ("&#x91", "\u2018"); // LEFT SINGLE QUOTATION MARK (‘)
			TestDecodeNumericEntity ("&#x92", "\u2019"); // RIGHT SINGLE QUOTATION MARK (’)
			TestDecodeNumericEntity ("&#x93", "\u201C"); // LEFT DOUBLE QUOTATION MARK (“)
			TestDecodeNumericEntity ("&#x94", "\u201D"); // RIGHT DOUBLE QUOTATION MARK (”)
			TestDecodeNumericEntity ("&#x95", "\u2022"); // BULLET (•)
			TestDecodeNumericEntity ("&#x96", "\u2013"); // EN DASH (–)
			TestDecodeNumericEntity ("&#x97", "\u2014"); // EM DASH (—)
			TestDecodeNumericEntity ("&#x98", "\u02DC"); // SMALL TILDE (˜)
			TestDecodeNumericEntity ("&#x99", "\u2122"); // TRADE MARK SIGN (™)
			TestDecodeNumericEntity ("&#x9A", "\u0161"); // LATIN SMALL LETTER S WITH CARON (š)
			TestDecodeNumericEntity ("&#x9B", "\u203A"); // SINGLE RIGHT-POINTING ANGLE QUOTATION MARK (›)
			TestDecodeNumericEntity ("&#x9C", "\u0153"); // LATIN SMALL LIGATURE OE (œ)
			TestDecodeNumericEntity ("&#x9E", "\u017E"); // LATIN SMALL LETTER Z WITH CARON (ž)
			TestDecodeNumericEntity ("&#x9F", "\u0178"); // LATIN CAPITAL LETTER Y WITH DIAERESIS (Ÿ)

			// parse error
			TestDecodeNumericEntity ("&#X10FFFF", "&#X10FFFF");

			TestDecodeNumericEntity ("&#xD800", "\uFFFD");

			TestDecodeNumericEntity ("&#1", "&#1");

			TestDecodeNumericEntity ("&#32", " ");
			TestDecodeNumericEntity ("&#x7a", "z");
		}
	}
}
