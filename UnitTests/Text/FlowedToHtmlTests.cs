//
// FlowedToHtmlTests.cs
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
	public class FlowedToHtmlTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var converter = new FlowedToHtml ();
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
			var converter = new FlowedToHtml ();

			Assert.IsFalse (converter.DeleteSpace, "DeleteSpace");
			Assert.IsFalse (converter.DetectEncodingFromByteOrderMark, "DetectEncodingFromByteOrderMark");
			Assert.IsNull (converter.Footer, "Footer");
			Assert.AreEqual (HeaderFooterFormat.Text, converter.FooterFormat, "FooterFormat");
			Assert.IsNull (converter.Header, "Header");
			Assert.AreEqual (HeaderFooterFormat.Text, converter.HeaderFormat, "HeaderFormat");
			Assert.IsNull (converter.HtmlTagCallback, "HtmlTagCallback");
			Assert.AreEqual (Encoding.UTF8, converter.InputEncoding, "InputEncoding");
			Assert.AreEqual (TextFormat.Flowed, converter.InputFormat, "InputFormat");
			Assert.AreEqual (4096, converter.InputStreamBufferSize, "InputStreamBufferSize");
			Assert.AreEqual (Encoding.UTF8, converter.OutputEncoding, "OutputEncoding");
			Assert.AreEqual (TextFormat.Html, converter.OutputFormat, "OutputFormat");
			Assert.IsFalse (converter.OutputHtmlFragment, "OutputHtmlFragment");
			Assert.AreEqual (4096, converter.OutputStreamBufferSize, "OutputStreamBufferSize");
		}

		[Test]
		public void TestSimpleFlowedToHtml ()
		{
			string expected = "<p>This is some sample text that has been formatted " +
				"according to the format=flowed rules defined in rfc3676. " +
				"This text, once converted, should all be on a single line.</p>" + Environment.NewLine +
				"<br/>" + Environment.NewLine +
				"<br/>" + Environment.NewLine +
				"<br/>" + Environment.NewLine +
				"<br/>" + Environment.NewLine +
				"<p>And this line of text should be separate by 4 blank lines.</p>" + Environment.NewLine;
			string text = "This is some sample text that has been formatted " + Environment.NewLine +
				"according to the format=flowed rules defined in rfc3676. " + Environment.NewLine +
				"This text, once converted, should all be on a single line." + Environment.NewLine +
				Environment.NewLine +
				Environment.NewLine +
				Environment.NewLine +
				Environment.NewLine +
				"And this line of text should be separate by 4 blank lines." + Environment.NewLine;
			var converter = new FlowedToHtml { OutputHtmlFragment = true };
			var result = converter.Convert (text);

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestIncreasingQuoteLevels ()
		{
			string expected = "<blockquote><p>Thou villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!</p>" + Environment.NewLine +
				"<blockquote><p>Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!</p>" + Environment.NewLine +
				"<blockquote><p>Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!</p>" + Environment.NewLine +
				"<blockquote><p>Henceforth, the coding style is to be strictly enforced, including the use of only upper case.</p>" + Environment.NewLine +
				"<blockquote><p>I&#39;ve noticed a lack of adherence to the coding styles, of late.</p>" + Environment.NewLine +
				"<blockquote><p>Any complaints?</p>" + Environment.NewLine +
				"</blockquote></blockquote></blockquote></blockquote></blockquote></blockquote>";
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
			var converter = new FlowedToHtml { OutputHtmlFragment = true };
			var result = converter.Convert (text);

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestDecreasingQuoteLevels ()
		{
			string expected = "<blockquote><blockquote><blockquote><blockquote><blockquote><blockquote><p>Thou villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!</p>" + Environment.NewLine +
				"</blockquote><p>Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!</p>" + Environment.NewLine +
				"</blockquote><p>Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!</p>" + Environment.NewLine +
				"</blockquote><p>Henceforth, the coding style is to be strictly enforced, including the use of only upper case.</p>" + Environment.NewLine +
				"</blockquote><p>I&#39;ve noticed a lack of adherence to the coding styles, of late.</p>" + Environment.NewLine +
				"</blockquote><p>Any complaints?</p>" + Environment.NewLine +
				"</blockquote>";
			string text = ">>>>>> Thou villainous ill-breeding spongy dizzy-eyed " + Environment.NewLine +
				">>>>>> reeky elf-skinned pigeon-egg!" + Environment.NewLine +
				">>>>> Thou artless swag-bellied milk-livered " + Environment.NewLine +
				">>>>> dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>>> Thou errant folly-fallen spleeny reeling-ripe " + Environment.NewLine +
				">>>> unmuzzled ratsbane!" + Environment.NewLine +
				">>> Henceforth, the coding style is to be strictly " + Environment.NewLine +
				">>> enforced, including the use of only upper case." + Environment.NewLine +
				">> I've noticed a lack of adherence to the coding " + Environment.NewLine +
				">> styles, of late." + Environment.NewLine +
				"> Any complaints?" + Environment.NewLine;
			var converter = new FlowedToHtml { OutputHtmlFragment = true };
			var result = converter.Convert (text);

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestBrokenlyQuotedText ()
		{
			// Note: this is the brokenly quoted sample from rfc3676 at the end of section 4.5
			string expected = "<blockquote><p>Thou villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg! </p>" + Environment.NewLine +
				"<blockquote><p>Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!</p>" + Environment.NewLine +
				"<blockquote><p>Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!</p>" + Environment.NewLine +
				"<blockquote><p>Henceforth, the coding style is to be strictly enforced, including the use of only upper case.</p>" + Environment.NewLine +
				"<blockquote><p>I&#39;ve noticed a lack of adherence to the coding styles, of late.</p>" + Environment.NewLine +
				"<blockquote><p>Any complaints?</p>" + Environment.NewLine +
				"</blockquote></blockquote></blockquote></blockquote></blockquote></blockquote>";
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
			var converter = new FlowedToHtml { OutputHtmlFragment = true };
			var result = converter.Convert (text);

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestTextHeaderAndFooter ()
		{
			string expected = "<html><body>On &lt;date&gt;, so-and-so said:<br/><blockquote><p>Thou villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!</p>" + Environment.NewLine +
				"<blockquote><p>Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!</p>" + Environment.NewLine +
				"<blockquote><p>Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!</p>" + Environment.NewLine +
				"<blockquote><p>Henceforth, the coding style is to be strictly enforced, including the use of only upper case.</p>" + Environment.NewLine +
				"<blockquote><p>I&#39;ve noticed a lack of adherence to the coding styles, of late.</p>" + Environment.NewLine +
				"<blockquote><p>Any complaints?</p>" + Environment.NewLine +
				"</blockquote></blockquote></blockquote></blockquote></blockquote></blockquote>Tha-tha-tha-tha that&#39;s all, folks!<br/></body></html>";
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
			var converter = new FlowedToHtml {
				Header = "On <date>, so-and-so said:" + Environment.NewLine,
				HeaderFormat = HeaderFooterFormat.Text,
				Footer = "Tha-tha-tha-tha that's all, folks!" + Environment.NewLine,
				FooterFormat = HeaderFooterFormat.Text,
				HtmlTagCallback = null
			};
			var result = converter.Convert (text);

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestSimpleFlowedWithUrlsToHtml ()
		{
			string expected = "<p>Check out <a href=\"http://www.xamarin.com\">http://www.xamarin.com</a> - it&#39;s amazing!</p>" + Environment.NewLine;
			string text = "Check out http://www.xamarin.com - it's amazing!" + Environment.NewLine;
			var converter = new FlowedToHtml { Header = null, Footer = null, OutputHtmlFragment = true };
			var result = converter.Convert (text);

			Assert.AreEqual (expected, result);
		}
	}
}
