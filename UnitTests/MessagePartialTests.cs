//
// MessagePartialTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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

using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MessagePartialTests
	{
		static MimeMessage Load (string path)
		{
			using (var file = File.OpenRead (path)) {
				var parser = new MimeParser (file);
				return parser.ParseMessage ();
			}
		}

		[Test]
		public void TestReassemble ()
		{
			var message1 = Load ("../../TestData/partial/message-partial.1.msg.txt");
			var message2 = Load ("../../TestData/partial/message-partial.2.msg.txt");

			Assert.IsNotNull (message1, "Failed to parse message-partial.1.msg");
			Assert.IsNotNull (message2, "Failed to parse message-partial.2.msg");

			Assert.IsTrue (message1.Body is MessagePartial, "The body of message-partial.1.msg is not a message/partial");
			Assert.IsTrue (message2.Body is MessagePartial, "The body of message-partial.2.msg is not a message/partial");

			var partials = new MessagePartial[] { (MessagePartial) message1.Body, (MessagePartial) message2.Body };
			var message = MessagePartial.Join (partials);

			Assert.IsNotNull (message, "Failed to reconstruct the message");
			Assert.AreEqual ("{15_3779; Victoria & Cherry}: suzeFan - 2377h003.jpg", message.Subject, "Subjects do not match");
			Assert.IsTrue (message.Body is Multipart, "Parsed message body is not a multipart");

			var multipart = (Multipart) message.Body;
			Assert.AreEqual (2, multipart.Count, "Multipart does not contain the expected number of parts");

			var part = multipart[1] as MimePart;
			Assert.IsNotNull (part, "Second part is null or not a MimePart");
			Assert.IsTrue (part.ContentType.Matches ("image", "jpeg"), "Attachment is not an image/jpeg");
			Assert.AreEqual ("2377h003.jpg", part.FileName, "Attachment filename is not the expected value");
		}
	}
}
