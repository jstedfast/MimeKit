//
// TextConverterTests.cs
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

using MimeKit.Text;

using NUnit.Framework;

namespace UnitTests {
	[TestFixture]
	public class TextConverterTests
	{
		[Test]
		public void TestSimpleFlowedToText ()
		{
			string expected = "This is some sample text that has been formatted " +
				"according to the format=flowed rules defined in rfc3676. " +
				"This text, once converted, should all be on a single line." + Environment.NewLine;
			string text = "This is some sample text that has been formatted " + Environment.NewLine +
				"according to the format=flowed rules defined in rfc3676. " + Environment.NewLine +
				"This text, once converted, should all be on a single line." + Environment.NewLine;
			var converter = new FlowedToText ();
			var result = converter.Convert (text);

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestSimpleFlowedToHTML ()
		{
			const string expected = "<p>This is some sample text that has been formatted " +
				"according to the format=flowed rules defined in rfc3676. " +
				"This text, once converted, should all be on a single line.</p>";
			string text = "This is some sample text that has been formatted " + Environment.NewLine +
				"according to the format=flowed rules defined in rfc3676. " + Environment.NewLine +
				"This text, once converted, should all be on a single line." + Environment.NewLine;
			var converter = new FlowedToHtml ();
			var result = converter.Convert (text);

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestSimpleFlowedWithUrlsToHTML ()
		{
			const string expected = "<p>Check out <a href=\"http://www.xamarin.com\">http://www.xamarin.com</a> - it&#39;s amazing!</p>";
			string text = "Check out http://www.xamarin.com - it's amazing!" + Environment.NewLine;
			var converter = new FlowedToHtml ();
			var result = converter.Convert (text);

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestSimpleTextToText ()
		{
			string expected = "This is some sample text. This is line #1." + Environment.NewLine +
				"This is line #2." + Environment.NewLine +
				"And this is line #3." + Environment.NewLine;
			string text = "This is some sample text. This is line #1." + Environment.NewLine +
				"This is line #2." + Environment.NewLine +
				"And this is line #3." + Environment.NewLine;
			var converter = new TextToText ();
			var result = converter.Convert (text);

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestSimpleTextToHTML ()
		{
			const string expected = "This is some sample text. This is line #1.<br/>" +
				"This is line #2.<br/>" +
				"And this is line #3.<br/>";
			string text = "This is some sample text. This is line #1." + Environment.NewLine +
				"This is line #2." + Environment.NewLine +
				"And this is line #3." + Environment.NewLine;
			var converter = new TextToHtml ();
			var result = converter.Convert (text);

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestSimpleTextWithUrlsToHTML ()
		{
			const string expected = "Check out <a href=\"http://www.xamarin.com\">http://www.xamarin.com</a> - it&#39;s amazing!<br/>";
			string text = "Check out http://www.xamarin.com - it's amazing!" + Environment.NewLine;
			var converter = new TextToHtml ();
			var result = converter.Convert (text);

			Assert.AreEqual (expected, result);
		}
	}
}
