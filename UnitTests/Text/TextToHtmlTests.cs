//
// TextToHtmlTests.cs
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
using System.Text;

using MimeKit.Text;

using NUnit.Framework;

namespace UnitTests.Text {
	[TestFixture]
	public class TextToHtmlTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var converter = new TextToHtml ();
			var reader = new StringReader ("");
			var writer = new StringWriter ();

			Assert.AreEqual (TextFormat.Plain, converter.InputFormat);
			Assert.AreEqual (TextFormat.Html, converter.OutputFormat);
			Assert.IsFalse (converter.DetectEncodingFromByteOrderMark);
			Assert.IsFalse (converter.OutputHtmlFragment);
			Assert.IsNull (converter.Footer);
			Assert.IsNull (converter.Header);
			Assert.AreEqual (HeaderFooterFormat.Text, converter.FooterFormat);
			Assert.AreEqual (HeaderFooterFormat.Text, converter.HeaderFormat);

			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, writer));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (reader, (TextWriter) null));
		}

		[Test]
		public void TestOutputHtmlFragment ()
		{
			const string input = "This is the html body";
			const string expected = "<html><body>This is the html body<br/></body></html>";
			const string expected2 = "This is the html body<br/>";
			var converter = new TextToHtml ();
			string result;

			result = converter.Convert (input);
			Assert.AreEqual (expected, result);

			converter.OutputHtmlFragment = true;
			result = converter.Convert (input);
			Assert.AreEqual (expected2, result);
		}

		[Test]
		public void TestHeaderFooter ()
		{
			const string input = "This is the html body";
			const string header = "This is the header";
			const string footer = "This is the footer";
			var expected = "<html><body>" + header + "<br/>" + input + "<br/>" + footer + "<br/></body></html>";
			var converter = new TextToHtml {
				HeaderFormat = HeaderFooterFormat.Text,
				Header = header,
				FooterFormat = HeaderFooterFormat.Text,
				Footer = footer
			};

			var result = converter.Convert (input);
			Assert.AreEqual (expected, result);
		}
	}
}
