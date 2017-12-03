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
			Assert.AreEqual (1252, CharsetUtils.ParseCodePage ("cp1252"));
		}
	}
}
