//
// ParserOptionsTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

using MimeKit;

namespace UnitTests {
	class BrokenTextHtmlPart : TextPart
	{
		public BrokenTextHtmlPart () : base ("html")
		{
		}
	}

	class CustomTextHtmlPart : TextPart
	{
		public CustomTextHtmlPart (MimeEntityConstructorArgs args) : base (args)
		{
		}

		public CustomTextHtmlPart () : base ("html")
		{
		}
	}

	class CustomMessagePart : MessagePart
	{
		public CustomMessagePart (MimeEntityConstructorArgs args) : base (args)
		{
		}
	}

	[TestFixture]
	public class ParserOptionsTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var options = ParserOptions.Default.Clone ();

			Assert.Throws<ArgumentNullException> (() => options.RegisterMimeType (null, typeof (CustomTextHtmlPart)));
			Assert.Throws<ArgumentNullException> (() => options.RegisterMimeType ("text/html", null));
			Assert.Throws<ArgumentException> (() => options.RegisterMimeType ("text/html", typeof (string)));
			Assert.Throws<ArgumentException> (() => options.RegisterMimeType ("text/html", typeof (BrokenTextHtmlPart)));
		}

		[Test]
		public void TestMaxHeaderLength ()
		{
			var options = new ParserOptions ();

			Assert.That (options.MaxHeaderLength, Is.EqualTo (16 * 1024 * 1024), "Default MaxHeaderLength");

			Assert.Throws<ArgumentOutOfRangeException> (() => options.MaxHeaderLength = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => options.MaxHeaderLength = -1);

			options.MaxHeaderLength = 1;

			var clone = options.Clone ();

			Assert.That (clone.MaxHeaderLength, Is.EqualTo (1), "Cloned MaxHeaderLength");
		}

		[Test]
		public void TestParsingOfApplicationRtf ()
		{
			const string rawMimeData = @"Content-type: application/rtf

This is make-believe rtf data...";

			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (rawMimeData))) {
				var part = MimeEntity.Load (stream);

				Assert.That (part, Is.InstanceOf<TextPart> (), "Expected the application/rtf part to be parsed as TextPart.");
				var text = (TextPart) part;
				Assert.That (text.IsRichText, Is.True, "IsRichText");
			}
		}

		[Test]
		public void TestParsingOfMessageGlobalHeaders ()
		{
			const string rawMimeData = @"Content-type: message/global-headers

Date: Fri, 22 Jan 2016 8:44:05 -0500 (EST)
From: MimeKit Unit Tests <unit.tests@mimekit.org>
To: MimeKit Unit Tests <unit.tests@mimekit.org>
MIME-Version: 1.0
Content-type: text/plain
";

			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (rawMimeData))) {
				var part = MimeEntity.Load (stream);

				Assert.That (part, Is.InstanceOf<TextRfc822Headers> (), "Expected the message/global-headers part to be parsed as TextRfc822Headers.");
			}
		}

		[Test]
		public void TestParsingOfCustomType ()
		{
			var options = ParserOptions.Default.Clone ();

			options.RegisterMimeType ("text/html", typeof (CustomTextHtmlPart));

			// now clone the options to make sure that the cloned copy still has the registered custom type
			options = options.Clone ();

			using (var stream = new MemoryStream ()) {
				var text = new TextPart ("html") { Text = "<html>this is some html and stuff</html>" };

				text.WriteTo (stream);
				stream.Position = 0;

				var html = MimeEntity.Load (options, stream);

				Assert.That (html, Is.InstanceOf<CustomTextHtmlPart> (), "Expected the text/html part to use our custom type.");
			}
		}

		[Test]
		public async Task TestParsingOfCustomTypeAsync ()
		{
			var options = ParserOptions.Default.Clone ();

			options.RegisterMimeType ("text/html", typeof (CustomTextHtmlPart));

			// now clone the options to make sure that the cloned copy still has the registered custom type
			options = options.Clone ();

			using (var stream = new MemoryStream ()) {
				var text = new TextPart ("html") { Text = "<html>this is some html and stuff</html>" };

				text.WriteTo (stream);
				stream.Position = 0;

				var html = await MimeEntity.LoadAsync (options, stream);

				Assert.That (html, Is.InstanceOf<CustomTextHtmlPart> (), "Expected the text/html part to use our custom type.");
			}
		}

		[Test]
		public void TestCustomMessagePartHonorsMaxMimeDepth ()
		{
			const string rawMimeData = "Content-Type: message/rfc822\n\nFrom: Example <example@example.com>\n\nBody";
			var options = ParserOptions.Default.Clone ();
			options.MaxMimeDepth = 0;
			options.RegisterMimeType ("message/rfc822", typeof (CustomMessagePart));

			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (rawMimeData))) {
				var entity = MimeEntity.Load (options, stream);

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected max depth to prevent custom message/rfc822 construction.");
				Assert.That (entity, Is.Not.InstanceOf<CustomMessagePart> (), "Expected max depth to prevent custom message/rfc822 construction.");
			}
		}

		[Test]
		public void TestEncodedMessageGlobalHeadersFallsBackToMimePart ()
		{
			const string rawMimeData = "Content-Type: message/global-headers\nContent-Transfer-Encoding: base64\n\nRnJvbTogRXhhbXBsZSA8ZXhhbXBsZUBleGFtcGxlLmNvbT4K";

			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (rawMimeData))) {
				var entity = MimeEntity.Load (stream);

				Assert.That (entity, Is.InstanceOf<MimePart> (), "Expected encoded message/global-headers to be a MimePart.");
				Assert.That (entity, Is.Not.InstanceOf<TextRfc822Headers> (), "Expected encoded message/global-headers not to be TextRfc822Headers.");
			}
		}
	}
}
