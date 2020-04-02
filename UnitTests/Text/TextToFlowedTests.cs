﻿//
// TextToFlowedTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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
	public class TextToFlowedTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var converter = new TextToFlowed ();
			var reader = new StringReader ("");
			var writer = new StringWriter ();

			Assert.Throws<ArgumentNullException> (() => converter.InputEncoding = null);
			Assert.Throws<ArgumentNullException> (() => converter.OutputEncoding = null);

			Assert.Throws<ArgumentOutOfRangeException> (() => converter.InputStreamBufferSize = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.OutputStreamBufferSize = -1);

			Assert.Throws<ArgumentNullException> (() => converter.Convert (null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, writer));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (reader, (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (new StreamReader (Stream.Null), (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, new StreamWriter (Stream.Null)));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (new StreamReader (Stream.Null), (TextWriter) null));
		}

		[Test]
		public void TestDefaultPropertyValues ()
		{
			var converter = new TextToFlowed ();

			Assert.IsFalse (converter.DetectEncodingFromByteOrderMark, "DetectEncodingFromByteOrderMark");
			Assert.IsNull (converter.Footer, "Footer");
			Assert.IsNull (converter.Header, "Header");
			Assert.AreEqual (Encoding.UTF8, converter.InputEncoding, "InputEncoding");
			Assert.AreEqual (TextFormat.Text, converter.InputFormat, "InputFormat");
			Assert.AreEqual (4096, converter.InputStreamBufferSize, "InputStreamBufferSize");
			Assert.AreEqual (Encoding.UTF8, converter.OutputEncoding, "OutputEncoding");
			Assert.AreEqual (TextFormat.Flowed, converter.OutputFormat, "OutputFormat");
			Assert.AreEqual (4096, converter.OutputStreamBufferSize, "OutputStreamBufferSize");
		}

		[Test]
		public void TestSimpleTextToFlowed ()
		{
			string expected = "> Thou art a villainous ill-breeding spongy dizzy-eyed reeky elf-skinned " + Environment.NewLine +
				">  pigeon-egg!" + Environment.NewLine +
				">> Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>> Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!" + Environment.NewLine +
				">>>> Henceforth, the coding style is to be strictly enforced, including " + Environment.NewLine +
				">>>>  the use of only upper case." + Environment.NewLine +
				">>>>> I've noticed a lack of adherence to the coding styles, of late." + Environment.NewLine +
				">>>>>> Any complaints?" + Environment.NewLine;
			string text = "> Thou art a villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!" + Environment.NewLine +
				">> Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>> Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!" + Environment.NewLine +
				">>>> Henceforth, the coding style is to be strictly enforced, including the use of only upper case." + Environment.NewLine +
				">>>>> I've noticed a lack of adherence to the coding styles, of late." + Environment.NewLine +
				">>>>>> Any complaints?" + Environment.NewLine;
			TextConverter converter = new TextToFlowed { Header = null, Footer = null };
			string result = converter.Convert (text);

			Assert.AreEqual (expected, result);

			converter = new FlowedToText { DeleteSpace = true };
			result = converter.Convert (expected);

			Assert.AreEqual (text, result);
		}

		[Test]
		public void TestSpaceStuffingFromLine ()
		{
			string expected = "My favorite James Bond movie is" + Environment.NewLine +
				" From Russia with love." + Environment.NewLine;
			string text = "My favorite James Bond movie is" + Environment.NewLine +
				"From Russia with love." + Environment.NewLine;
			TextConverter converter = new TextToFlowed ();
			string result = converter.Convert (text);

			Assert.AreEqual (expected, result);

			converter = new FlowedToText ();
			result = converter.Convert (expected);

			Assert.AreEqual (text, result);
		}

		[Test]
		public void TestSpaceStuffingLinesStartingWithSpace ()
		{
			string expected = "This is a regular line." + Environment.NewLine +
				"  This line starts with a space." + Environment.NewLine;
			string text = "This is a regular line." + Environment.NewLine +
				" This line starts with a space." + Environment.NewLine;
			TextConverter converter = new TextToFlowed ();
			string result = converter.Convert (text);

			Assert.AreEqual (expected, result);

			converter = new FlowedToText ();
			result = converter.Convert (expected);

			Assert.AreEqual (text, result);
		}
	}
}
