//
// TextConverterTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
	public class TextConverterTests
	{
		[Test]
		public void TestPropertySetters ()
		{
			var latin1 = Encoding.GetEncoding ("iso-8859-1");
			var utf16 = Encoding.Unicode;
			var converter = new TextToText ();

			converter.InputEncoding = latin1;
			Assert.AreEqual (latin1, converter.InputEncoding, "InputEncoding");

			converter.InputStreamBufferSize = 5000;
			Assert.AreEqual (5000, converter.InputStreamBufferSize, "InputStreamBufferSize");

			converter.OutputEncoding = utf16;
			Assert.AreEqual (utf16, converter.OutputEncoding, "OutputEncoding");

			converter.OutputStreamBufferSize = 6000;
			Assert.AreEqual (6000, converter.OutputStreamBufferSize, "OutputStreamBufferSize");

			converter.DetectEncodingFromByteOrderMark = true;
			Assert.IsTrue (converter.DetectEncodingFromByteOrderMark, "DetectEncodingFromByteOrderMark");
		}

		[Test]
		public void TestConvertFromReaderToStream ()
		{
			const string input = "This is some text...";
			var converter = new TextToText {
				DetectEncodingFromByteOrderMark = false,
				InputEncoding = Encoding.ASCII,
				OutputEncoding = Encoding.ASCII
			};

			using (var output = new MemoryStream ()) {
				using (var reader = new StringReader (input))
					converter.Convert (reader, output);

				var result = Encoding.ASCII.GetString (output.GetBuffer (), 0, (int) output.Length);

				Assert.AreEqual (input, result);
			}
		}

		[Test]
		public void TestConvertFromStreamToStream ()
		{
			const string input = "This is some text...";
			var converter = new TextToText {
				DetectEncodingFromByteOrderMark = false,
				InputEncoding = Encoding.ASCII,
				OutputEncoding = Encoding.ASCII
			};

			using (var output = new MemoryStream ()) {
				using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (input)))
					converter.Convert (stream, output);

				var result = Encoding.ASCII.GetString (output.GetBuffer (), 0, (int) output.Length);

				Assert.AreEqual (input, result);
			}
		}

		[Test]
		public void TestConvertFromStreamToWriter ()
		{
			const string input = "This is some text...";
			var converter = new TextToText {
				DetectEncodingFromByteOrderMark = false,
				InputEncoding = Encoding.ASCII,
				OutputEncoding = Encoding.ASCII
			};

			using (var writer = new StringWriter ()) {
				using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (input)))
					converter.Convert (stream, writer);

				var result = writer.ToString ();

				Assert.AreEqual (input, result);
			}
		}
	}
}
