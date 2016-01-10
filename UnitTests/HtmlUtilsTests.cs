//
// HtmlUtilsTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (www.xamarin.com)
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

using HtmlKit;

namespace UnitTests {
	[TestFixture]
	public class HtmlUtilsTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var writer = new StringWriter ();
			const string text = "text";

			// HtmlAttributeEncode
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (null));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (text, 'x'));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (null, text));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (writer, null));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (writer, text, 'x'));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode ((string) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (text, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (text, 0, text.Length + 1));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (text, 0, text.Length, 'x'));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode ((char[]) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (text.ToCharArray (), -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (text.ToCharArray (), 0, text.Length + 1));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (text.ToCharArray (), 0, text.Length, 'x'));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (null, text, 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (writer, (string) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (writer, text, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (writer, text, 0, text.Length + 1));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (writer, text, 0, text.Length, 'x'));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (null, text.ToCharArray(), 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (writer, (char[]) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (writer, text.ToCharArray (), -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (writer, text.ToCharArray (), 0, text.Length + 1));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (writer, text.ToCharArray (), 0, text.Length, 'x'));

			// HtmlEncode
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (null));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (null, text));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (writer, null));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode ((string) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (text, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (text, 0, text.Length + 1));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode ((char[]) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (text.ToCharArray (), -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (text.ToCharArray (), 0, text.Length + 1));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (null, text, 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (writer, (string) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (writer, text, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (writer, text, 0, text.Length + 1));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (null, text.ToCharArray(), 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (writer, (char[]) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (writer, text.ToCharArray (), -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (writer, text.ToCharArray (), 0, text.Length + 1));

			// HtmlDecode
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlDecode (null));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlDecode (null, text));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlDecode (writer, null));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlDecode (null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlDecode (text, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlDecode (text, 0, text.Length + 1));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlDecode (null, text, 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlDecode (writer, null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlDecode (writer, text, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlDecode (writer, text, 0, text.Length + 1));
		}
	}
}
