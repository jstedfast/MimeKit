//
// TextToHtmlTests.cs
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

using MimeKit.Encodings;
using MimeKit.Text;

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
			var converter = new TextToHtml ();

			Assert.That (converter.DetectEncodingFromByteOrderMark, Is.False, "DetectEncodingFromByteOrderMark");
			Assert.That (converter.Footer, Is.Null, "Footer");
			Assert.That (converter.FooterFormat, Is.EqualTo (HeaderFooterFormat.Text), "FooterFormat");
			Assert.That (converter.Header, Is.Null, "Header");
			Assert.That (converter.HeaderFormat, Is.EqualTo (HeaderFooterFormat.Text), "HeaderFormat");
			Assert.That (converter.HtmlTagCallback, Is.Null, "HtmlTagCallback");
			Assert.That (converter.InputEncoding, Is.EqualTo (Encoding.UTF8), "InputEncoding");
			Assert.That (converter.InputFormat, Is.EqualTo (TextFormat.Text), "InputFormat");
			Assert.That (converter.InputStreamBufferSize, Is.EqualTo (4096), "InputStreamBufferSize");
			Assert.That (converter.OutputEncoding, Is.EqualTo (Encoding.UTF8), "OutputEncoding");
			Assert.That (converter.OutputFormat, Is.EqualTo (TextFormat.Html), "OutputFormat");
			Assert.That (converter.OutputHtmlFragment, Is.False, "OutputHtmlFragment");
			Assert.That (converter.OutputStreamBufferSize, Is.EqualTo (4096), "OutputStreamBufferSize");
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
			Assert.That (result, Is.EqualTo (expected));

			converter.OutputHtmlFragment = true;
			result = converter.Convert (input);
			Assert.That (result, Is.EqualTo (expected2));
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
			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestEmoji ()
		{
			var expected = "<html><body>&#128561;<br/></body></html>";
			var buffer = Encoding.ASCII.GetBytes ("=F0=9F=98=B1");
			var decoder = new QuotedPrintableDecoder ();
			var length = decoder.EstimateOutputLength (buffer.Length);
			var decoded = new byte[length];
			var n = decoder.Decode (buffer, 0, buffer.Length, decoded);
			var emoji = Encoding.UTF8.GetString (decoded, 0, n);
			var converter = new TextToHtml ();
			var result = converter.Convert (emoji);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestIncreasingQuoteLevels ()
		{
			string expected = "<blockquote>Thou villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!<br/>" +
				"<blockquote>Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!<br/>" +
				"<blockquote>Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!<br/>" +
				"<blockquote>Henceforth, the coding style is to be strictly enforced, including the use of only upper case.<br/>" +
				"<blockquote>I&#39;ve noticed a lack of adherence to the coding styles, of late.<br/>" +
				"<blockquote>Any complaints?<br/>" +
				"</blockquote></blockquote></blockquote></blockquote></blockquote></blockquote>";
			string text = "> Thou villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!" + Environment.NewLine +
				">> Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>> Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!" + Environment.NewLine +
				">>>> Henceforth, the coding style is to be strictly enforced, including the use of only upper case." + Environment.NewLine +
				">>>>> I've noticed a lack of adherence to the coding styles, of late." + Environment.NewLine +
				">>>>>> Any complaints?" + Environment.NewLine;
			var converter = new TextToHtml { OutputHtmlFragment = true };
			var result = converter.Convert (text);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestIncreasingQuoteLevelsNoNewLineAtEndOfText ()
		{
			string expected = "<blockquote>Thou villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!<br/>" +
				"<blockquote>Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!<br/>" +
				"<blockquote>Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!<br/>" +
				"<blockquote>Henceforth, the coding style is to be strictly enforced, including the use of only upper case.<br/>" +
				"<blockquote>I&#39;ve noticed a lack of adherence to the coding styles, of late.<br/>" +
				"<blockquote>Any complaints?<br/>" +
				"</blockquote></blockquote></blockquote></blockquote></blockquote></blockquote>";
			string text = "> Thou villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!" + Environment.NewLine +
				">> Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>> Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!" + Environment.NewLine +
				">>>> Henceforth, the coding style is to be strictly enforced, including the use of only upper case." + Environment.NewLine +
				">>>>> I've noticed a lack of adherence to the coding styles, of late." + Environment.NewLine +
				">>>>>> Any complaints?";
			var converter = new TextToHtml { OutputHtmlFragment = true };
			var result = converter.Convert (text);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestDecreasingQuoteLevels ()
		{
			string expected = "<blockquote><blockquote><blockquote><blockquote><blockquote><blockquote>Thou villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!<br/>" +
				"</blockquote>Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!<br/>" +
				"</blockquote>Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!<br/>" +
				"</blockquote>Henceforth, the coding style is to be strictly enforced, including the use of only upper case.<br/>" +
				"</blockquote>I&#39;ve noticed a lack of adherence to the coding styles, of late.<br/>" +
				"</blockquote>Any complaints?<br/>" +
				"</blockquote>";
			string text = ">>>>>> Thou villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!" + Environment.NewLine +
				">>>>> Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>>> Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!" + Environment.NewLine +
				">>> Henceforth, the coding style is to be strictly enforced, including the use of only upper case." + Environment.NewLine +
				">> I've noticed a lack of adherence to the coding styles, of late." + Environment.NewLine +
				"> Any complaints?" + Environment.NewLine;
			var converter = new TextToHtml { OutputHtmlFragment = true };
			var result = converter.Convert (text);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestSimpleTextToHtml ()
		{
			const string expected = "This is some sample text. This is line #1.<br/>" +
				"This is line #2.<br/>" +
				"And this is line #3.<br/>";
			string text = "This is some sample text. This is line #1." + Environment.NewLine +
				"This is line #2." + Environment.NewLine +
				"And this is line #3." + Environment.NewLine;
			var converter = new TextToHtml { OutputHtmlFragment = true };
			var result = converter.Convert (text);

			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestSimpleTextWithUrlsToHtml ()
		{
			const string expected = "Check out <a href=\"http://www.xamarin.com\">http://www.xamarin.com</a> - it&#39;s amazing!<br/>";
			string text = "Check out http://www.xamarin.com - it's amazing!" + Environment.NewLine;
			var converter = new TextToHtml { OutputHtmlFragment = true };
			var result = converter.Convert (text);

			Assert.That (result, Is.EqualTo (expected));
		}
	}
}
