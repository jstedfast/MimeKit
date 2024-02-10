//
// TextToTextTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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

using MimeKit.Text;

namespace UnitTests.Text {
	[TestFixture]
	public class TextToTextTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var converter = new TextToText ();

			Assert.Throws<ArgumentNullException> (() => converter.InputEncoding = null);
			Assert.Throws<ArgumentNullException> (() => converter.OutputEncoding = null);

			Assert.Throws<ArgumentOutOfRangeException> (() => converter.InputStreamBufferSize = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.OutputStreamBufferSize = -1);

			Assert.Throws<ArgumentNullException> (() => converter.Convert (null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (new StreamReader (Stream.Null), (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, new StreamWriter (Stream.Null)));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (new StreamReader (Stream.Null), (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, new StreamWriter (Stream.Null)));
		}

		[Test]
		public void TestDefaultPropertyValues ()
		{
			var converter = new TextToText ();

			Assert.That (converter.DetectEncodingFromByteOrderMark, Is.False, "DetectEncodingFromByteOrderMark");
			Assert.That (converter.Footer, Is.Null, "Footer");
			Assert.That (converter.Header, Is.Null, "Header");
			Assert.That (converter.InputEncoding, Is.EqualTo (Encoding.UTF8), "InputEncoding");
			Assert.That (converter.InputFormat, Is.EqualTo (TextFormat.Text), "InputFormat");
			Assert.That (converter.OutputEncoding, Is.EqualTo (Encoding.UTF8), "OutputEncoding");
			Assert.That (converter.OutputFormat, Is.EqualTo (TextFormat.Text), "OutputFormat");
			Assert.That (converter.InputStreamBufferSize, Is.EqualTo (4096), "InputStreamBufferSize");
			Assert.That (converter.OutputStreamBufferSize, Is.EqualTo (4096), "OutputStreamBufferSize");
		}

		[Test]
		public void TestHeaderAndFooter ()
		{
			string expected = "Header,Footer";
			string text = ",";
			var converter = new TextToText { Header = "Header", Footer = "Footer" };
			var result = converter.Convert (text);

			Assert.That (result, Is.EqualTo (expected));
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

			Assert.That (result, Is.EqualTo (expected));
		}
	}
}
