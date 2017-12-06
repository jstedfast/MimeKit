//
// CharsetUtilsTests.cs
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
using System.Text;
using System.Security.Cryptography;

using NUnit.Framework;

using MimeKit;
using MimeKit.Utils;

namespace UnitTests.Utils {
	[TestFixture]
	public class CharsetUtilsTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var buffer = new byte[10];

			Assert.Throws<ArgumentNullException> (() => CharsetUtils.ConvertToUnicode ((ParserOptions) null, buffer, 0, buffer.Length));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.ConvertToUnicode (ParserOptions.Default, null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => CharsetUtils.ConvertToUnicode (ParserOptions.Default, buffer, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => CharsetUtils.ConvertToUnicode (ParserOptions.Default, buffer, 0, -1));

			Assert.Throws<ArgumentNullException> (() => CharsetUtils.ConvertToUnicode ((Encoding) null, buffer, 0, buffer.Length));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.ConvertToUnicode (Encoding.UTF8, null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => CharsetUtils.ConvertToUnicode (Encoding.UTF8, buffer, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => CharsetUtils.ConvertToUnicode (Encoding.UTF8, buffer, 0, -1));

			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetCodePage (null));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetEncoding (null));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetEncoding (null, "fallback"));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetEncoding ("charset", null));
			Assert.Throws<ArgumentOutOfRangeException> (() => CharsetUtils.GetEncoding (-1, "fallback"));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetEncoding (28591, null));

			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetMimeCharset ((string) null));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetMimeCharset ((Encoding) null));
		}

		[Test]
		public void TestParseCodePage ()
		{
			Assert.AreEqual (1201, CharsetUtils.ParseCodePage ("iso10646"));
			Assert.AreEqual (1201, CharsetUtils.ParseCodePage ("iso-10646"));
			Assert.AreEqual (1201, CharsetUtils.ParseCodePage ("iso10646-1"));
			Assert.AreEqual (1201, CharsetUtils.ParseCodePage ("iso-10646-1"));

			Assert.AreEqual (28591, CharsetUtils.ParseCodePage ("iso8859-1"));
			Assert.AreEqual (28591, CharsetUtils.ParseCodePage ("iso8859_1"));
			Assert.AreEqual (28591, CharsetUtils.ParseCodePage ("iso-8859-1"));
			Assert.AreEqual (28591, CharsetUtils.ParseCodePage ("iso_8859_1"));
			Assert.AreEqual (28591, CharsetUtils.ParseCodePage ("latin1"));

			Assert.AreEqual (50220, CharsetUtils.ParseCodePage ("iso2022-jp"));
			Assert.AreEqual (50220, CharsetUtils.ParseCodePage ("iso-2022-jp"));
			Assert.AreEqual (50220, CharsetUtils.ParseCodePage ("iso_2022_jp"));
			Assert.AreEqual (50225, CharsetUtils.ParseCodePage ("iso2022-kr"));
			Assert.AreEqual (50225, CharsetUtils.ParseCodePage ("iso-2022-kr"));
			Assert.AreEqual (50225, CharsetUtils.ParseCodePage ("iso_2022_kr"));

			Assert.AreEqual (1252, CharsetUtils.ParseCodePage ("windows-cp1252"));
			Assert.AreEqual (1252, CharsetUtils.ParseCodePage ("windows-1252"));
			Assert.AreEqual (1252, CharsetUtils.ParseCodePage ("cp-1252"));
			Assert.AreEqual (1252, CharsetUtils.ParseCodePage ("cp1252"));

			Assert.AreEqual (-1, CharsetUtils.ParseCodePage ("cp"));
			Assert.AreEqual (-1, CharsetUtils.ParseCodePage ("iso"));
			Assert.AreEqual (-1, CharsetUtils.ParseCodePage ("ibm"));
			Assert.AreEqual (-1, CharsetUtils.ParseCodePage ("windows"));
			Assert.AreEqual (-1, CharsetUtils.ParseCodePage ("windows-"));

			Assert.AreEqual (-1, CharsetUtils.ParseCodePage ("iso-8859"));
			Assert.AreEqual (-1, CharsetUtils.ParseCodePage ("iso-BB59"));
			Assert.AreEqual (-1, CharsetUtils.ParseCodePage ("iso-8859-A"));
			Assert.AreEqual (-1, CharsetUtils.ParseCodePage ("iso-2022-US"));
			Assert.AreEqual (-1, CharsetUtils.ParseCodePage ("iso-4999-1"));
		}

		[Test]
		public void TestConvertToUnicode ()
		{
			const string expected = "要連両存使破薫聞載線弁明小設幸名代開告覧。胸問中写視映的収掲笛事来更知。会教握話整団負画断問囲士英高枠営言。近着開交識営害新緑提犯趣大第快者一代田。海補間山間府序打面真教義優決位。需著会容同警士趣場社主交干今兆。電増罪合三時株再姿県人生。広暮分昭熊勢戦帯票昨切議読権月期春著。生上呼警上際岡作朝米趙情。";
			var options = ParserOptions.Default.Clone ();
			var gb2312 = Encoding.GetEncoding (936);

			options.CharsetEncoding = gb2312;

			var input = gb2312.GetBytes (expected);

			var actual = CharsetUtils.ConvertToUnicode (options, input, 0, input.Length);

			Assert.AreEqual (expected, actual);
		}

		[Test]
		public void TestGetMimeCharset ()
		{
			Encoding encoding;

			Assert.AreEqual ("iso-8859-1", CharsetUtils.GetMimeCharset ("latin1"));
			Assert.AreEqual ("iso-8859-1", CharsetUtils.GetMimeCharset (CharsetUtils.Latin1));
			Assert.AreEqual ("gibberish", CharsetUtils.GetMimeCharset ("gibberish"));

			Assert.AreEqual ("iso-2022-jp", CharsetUtils.GetMimeCharset (Encoding.GetEncoding (932)));
			Assert.AreEqual ("iso-2022-jp", CharsetUtils.GetMimeCharset (Encoding.GetEncoding (50220)));
			Assert.AreEqual ("iso-2022-jp", CharsetUtils.GetMimeCharset (Encoding.GetEncoding (50221)));

			try {
				encoding = Encoding.GetEncoding (50225);
			} catch (NotSupportedException) {
				encoding = null;
			}

			if (encoding != null)
				Assert.AreEqual ("euc-kr", CharsetUtils.GetMimeCharset (encoding));

			Assert.AreEqual ("euc-kr", CharsetUtils.GetMimeCharset (Encoding.GetEncoding (949)));
		}
	}
}
