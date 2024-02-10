//
// TextRfc822HeadersTests.cs
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

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class TextRfc822HeadersTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var entity = new TextRfc822Headers ();

			Assert.Throws<ArgumentException> (() => new TextRfc822Headers ("unknown-parameter"));
			Assert.Throws<ArgumentException> (() => new TextRfc822Headers (new MimeMessage (), new MimeMessage ()));

			Assert.Throws<ArgumentNullException> (() => entity.Accept (null));
		}

		class TextRfc822HeadersVisitor : MimeVisitor
		{
			public TextRfc822Headers Rfc822Headers;

			protected internal override void VisitTextRfc822Headers (TextRfc822Headers entity)
			{
				Rfc822Headers = entity;
			}
		}

		[Test]
		public void TestSerializationAndDeserialization ()
		{
			var message = new MimeMessage ();
			message.From.Add (new MailboxAddress ("Sender Name", "sender@example.com"));
			message.To.Add (new MailboxAddress ("Recipient Name", "recipient@example.com"));
			message.Subject = "Content of a text/rfc822-headers part";

			var rfc822headers = new TextRfc822Headers (new Header (HeaderId.ContentId, "<id@localhost>"), message);

			message = new MimeMessage ();
			message.From.Add (new MailboxAddress ("Postmaster", "postmaster@example.com"));
			message.To.Add (new MailboxAddress ("Sender Name", "sender@example.com.com"));
			message.Subject = "Sorry, but your message bounced";
			message.Body = rfc822headers;

			using (var stream = new MemoryStream ()) {
				message.WriteTo (stream);
				stream.Position = 0;

				message = MimeMessage.Load (stream);

				var visitor = new TextRfc822HeadersVisitor ();
				visitor.Visit (message);

				Assert.That (visitor.Rfc822Headers, Is.Not.Null, "Rfc822Headers");
				Assert.That (visitor.Rfc822Headers.ContentId, Is.EqualTo ("id@localhost"), "ContentId");
			}
		}
	}
}
