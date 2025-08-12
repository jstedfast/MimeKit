//
// BodyBuilderTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
using MimeKit.Utils;

namespace UnitTests {
	[TestFixture]
	public class BodyBuilderTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => new BodyBuilder { BodyEncoding = null });
		}

		[Test]
		public void TestToMessageBodyDefault ()
		{
			var bodyBuilder = new BodyBuilder ();

			Assert.That (bodyBuilder.BodyEncoding, Is.EqualTo (Encoding.UTF8), "Default BodyEncoding should be UTF-8.");

			var body = bodyBuilder.ToMessageBody ();

			Assert.That (body, Is.InstanceOf<TextPart> (), "Body should be a TextPart.");

			var textBody = (TextPart) body;
			Assert.That (textBody.ContentType.MimeType, Is.EqualTo ("text/plain"), "Content-Type mime-type does not match.");
			Assert.That (textBody.ContentType.Charset, Is.EqualTo ("utf-8"), "Content-Type charset does not match.");
			Assert.That (textBody.Text, Is.EqualTo (string.Empty), "Text body does not match.");
		}

		[Test]
		public void TestBodyEncodingTextPlain ()
		{
			var bodyBuilder = new BodyBuilder {
				BodyEncoding = CharsetUtils.Latin1,
				TextBody = "This is the text body."
			};
			var body = bodyBuilder.ToMessageBody ();

			Assert.That (body, Is.InstanceOf<TextPart> (), "Body should be a TextPart.");

			var textBody = (TextPart) body;
			Assert.That (textBody.ContentType.MimeType, Is.EqualTo ("text/plain"), "Content-Type mime-type does not match.");
			Assert.That (textBody.ContentType.Charset, Is.EqualTo ("iso-8859-1"), "Content-Type charset does not match.");
			Assert.That (textBody.Text, Is.EqualTo (bodyBuilder.TextBody), "Text body does not match.");
		}

		[Test]
		public void TestBodyEncodingTextHtml ()
		{
			var bodyBuilder = new BodyBuilder {
				BodyEncoding = CharsetUtils.Latin1,
				HtmlBody = "This is the html body."
			};
			var body = bodyBuilder.ToMessageBody ();

			Assert.That (body, Is.InstanceOf<TextPart> (), "Body should be a TextPart.");

			var textBody = (TextPart) body;
			Assert.That (textBody.ContentType.MimeType, Is.EqualTo ("text/html"), "Content-Type mime-type does not match.");
			Assert.That (textBody.ContentType.Charset, Is.EqualTo ("iso-8859-1"), "Content-Type charset does not match.");
			Assert.That (textBody.Text, Is.EqualTo (bodyBuilder.HtmlBody), "Text body does not match.");
		}

		[Test]
		public void TestBodyEncodingMultipartAlternative ()
		{
			var bodyBuilder = new BodyBuilder {
				BodyEncoding = CharsetUtils.Latin1,
				TextBody = "This is the text body.",
				HtmlBody = "This is the html body."
			};
			var body = bodyBuilder.ToMessageBody ();

			Assert.That (body, Is.InstanceOf<MultipartAlternative> (), "Body should be a MultipartAlternative.");

			var alternative = (MultipartAlternative) body;
			Assert.That (alternative.Count, Is.EqualTo (2), "MultipartAlternative should contain 2 parts.");
			Assert.That (alternative[0], Is.InstanceOf<TextPart> (), "First part should be a TextPart.");
			Assert.That (alternative[1], Is.InstanceOf<TextPart> (), "Second part should be a TextPart.");

			var textBody = (TextPart) alternative[0];
			Assert.That (textBody.ContentType.MimeType, Is.EqualTo ("text/plain"), "Content-Type mime-type does not match.");
			Assert.That (textBody.ContentType.Charset, Is.EqualTo ("iso-8859-1"), "Content-Type charset does not match.");
			Assert.That (textBody.Text, Is.EqualTo (bodyBuilder.TextBody), "Text body does not match.");

			var htmlBody = (TextPart) alternative[1];
			Assert.That (htmlBody.ContentType.MimeType, Is.EqualTo ("text/html"), "Content-Type mime-type does not match.");
			Assert.That (htmlBody.ContentType.Charset, Is.EqualTo ("iso-8859-1"), "Content-Type charset does not match.");
			Assert.That (htmlBody.Text, Is.EqualTo (bodyBuilder.HtmlBody), "HTML body does not match.");
		}
	}
}
