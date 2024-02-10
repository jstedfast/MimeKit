//
// FlowedToTextTests.cs
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
	public class FlowedToTextTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var converter = new FlowedToText ();
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
			var converter = new FlowedToText ();

			Assert.That (converter.DeleteSpace, Is.False, "DeleteSpace");
			Assert.That (converter.DetectEncodingFromByteOrderMark, Is.False, "DetectEncodingFromByteOrderMark");
			Assert.That (converter.Footer, Is.Null, "Footer");
			Assert.That (converter.Header, Is.Null, "Header");
			Assert.That (converter.InputEncoding, Is.EqualTo (Encoding.UTF8), "InputEncoding");
			Assert.That (converter.InputFormat, Is.EqualTo (TextFormat.Flowed), "InputFormat");
			Assert.That (converter.InputStreamBufferSize, Is.EqualTo (4096), "InputStreamBufferSize");
			Assert.That (converter.OutputEncoding, Is.EqualTo (Encoding.UTF8), "OutputEncoding");
			Assert.That (converter.OutputFormat, Is.EqualTo (TextFormat.Text), "OutputFormat");
			Assert.That (converter.OutputStreamBufferSize, Is.EqualTo (4096), "OutputStreamBufferSize");
		}

		[Test]
		public void TestSimpleFlowedToText ()
		{
			string expected = "This is some sample text that has been formatted " +
				"according to the format=flowed rules defined in rfc3676. " +
				"This text, once converted, should all be on a single line." + Environment.NewLine;
			string text = "This is some sample text that has been formatted " + Environment.NewLine +
				"according to the format=flowed rules defined in rfc3676. " + Environment.NewLine +
				"This text, once converted, should all be on a single line." + Environment.NewLine;
			var converter = new FlowedToText { Header = null, Footer = null };
			var result = converter.Convert (text);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestQuotedFlowedToText ()
		{
			string expected = "> Thou villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!" + Environment.NewLine +
				">> Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>> Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!" + Environment.NewLine +
				">>>> Henceforth, the coding style is to be strictly enforced, including the use of only upper case." + Environment.NewLine +
				">>>>> I've noticed a lack of adherence to the coding styles, of late." + Environment.NewLine +
				">>>>>> Any complaints?" + Environment.NewLine;
			string text = "> Thou villainous ill-breeding spongy dizzy-eyed " + Environment.NewLine +
				"> reeky elf-skinned pigeon-egg!" + Environment.NewLine +
				">> Thou artless swag-bellied milk-livered " + Environment.NewLine +
				">> dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>> Thou errant folly-fallen spleeny reeling-ripe " + Environment.NewLine +
				">>> unmuzzled ratsbane!" + Environment.NewLine +
				">>>> Henceforth, the coding style is to be strictly " + Environment.NewLine +
				">>>> enforced, including the use of only upper case." + Environment.NewLine +
				">>>>> I've noticed a lack of adherence to the coding " + Environment.NewLine +
				">>>>> styles, of late." + Environment.NewLine +
				">>>>>> Any complaints?" + Environment.NewLine;
			var converter = new FlowedToText { Header = null, Footer = null };
			var result = converter.Convert (text);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestBrokenQuotedFlowedToText ()
		{
			// Note: this is the brokenly quoted sample from rfc3676 at the end of section 4.5
			string expected = "> Thou villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg! " + Environment.NewLine +
				">> Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>> Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!" + Environment.NewLine +
				">>>> Henceforth, the coding style is to be strictly enforced, including the use of only upper case." + Environment.NewLine +
				">>>>> I've noticed a lack of adherence to the coding styles, of late." + Environment.NewLine +
				">>>>>> Any complaints?" + Environment.NewLine;
			string text = "> Thou villainous ill-breeding spongy dizzy-eyed " + Environment.NewLine +
				"> reeky elf-skinned pigeon-egg! " + Environment.NewLine +
				">> Thou artless swag-bellied milk-livered " + Environment.NewLine +
				">> dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>> Thou errant folly-fallen spleeny reeling-ripe " + Environment.NewLine +
				">>> unmuzzled ratsbane!" + Environment.NewLine +
				">>>> Henceforth, the coding style is to be strictly " + Environment.NewLine +
				">>>> enforced, including the use of only upper case." + Environment.NewLine +
				">>>>> I've noticed a lack of adherence to the coding " + Environment.NewLine +
				">>>>> styles, of late." + Environment.NewLine +
				">>>>>> Any complaints?" + Environment.NewLine;
			var converter = new FlowedToText { Header = null, Footer = null };
			var result = converter.Convert (text);

			Assert.That (result, Is.EqualTo (expected));
		}
	}
}
