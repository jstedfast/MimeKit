//
// ConstructorTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class ConstructorTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var body = new TextPart ("plain") { Text = "This is the body..." };
			var message = new MimeMessage ();

			Assert.Throws<ArgumentNullException> (() => new MimeMessage ((object[]) null));
			Assert.Throws<ArgumentException> (() => new MimeMessage (body, null, body));
			Assert.Throws<ArgumentException> (() => new MimeMessage (5));

			Assert.Throws<ArgumentNullException> (() => new MessagePart ("rfc822", (object[]) null));
			Assert.Throws<ArgumentException> (() => new MessagePart ("rfc822", message, null, message));
			Assert.Throws<ArgumentException> (() => new MessagePart ("rfc822", 5));

			Assert.Throws<ArgumentNullException> (() => new MimePart ("text", "plain", (object[]) null));
			Assert.Throws<ArgumentException> (() => new MimePart ("text", "plain", body.Content, body.Content.Stream));
			Assert.Throws<ArgumentException> (() => new MimePart ("text", "plain", body.Content.Stream, body.Content));
			Assert.Throws<ArgumentException> (() => new MimePart ("text", "plain", null, 5));
		}

		[Test]
		public void TestMimeMessageWithHeaders ()
		{
			var msg = new MimeMessage (
				new Header ("From", "Federico Di Gregorio <fog@dndg.it>"),
				new Header ("To", "jeff@xamarin.com"),
				new [] { new Header ("Cc", "fog@dndg.it"), new Header ("Cc", "<gg@dndg.it>") },
				new Header ("Subject", "Hello"),
				new TextPart ("plain", "Just a short message to say hello!")
			);

			Assert.AreEqual (1, msg.From.Count, "Wrong count in From");
			Assert.AreEqual ("\"Federico Di Gregorio\" <fog@dndg.it>", msg.From[0].ToString(), "Wrong value in From[0]");
			Assert.AreEqual (1, msg.To.Count, "Wrong count in To");
			Assert.AreEqual ("jeff@xamarin.com", msg.To[0].ToString(), "Wrong value in To[0]");
			Assert.AreEqual (2, msg.Cc.Count, 2, "Wrong count in Cc");
			Assert.AreEqual ("fog@dndg.it", msg.Cc[0].ToString(), "Wrong value in Cc[0]");
			Assert.AreEqual ("gg@dndg.it", msg.Cc[1].ToString(), "Wrong value in Cc[1]");
			Assert.AreEqual ("Hello", msg.Subject, "Wrong value in Subject");		
		}

		[Test]
		public void TestGenerateMultipleMessagesWithLinq ()
		{
			string[] destinations = { "jeff@xamarin.com", "gg@dndg.it" };

			IList<MimeMessage> msgs = destinations.Select(x => new MimeMessage(
				new Header ("From", "Federico Di Gregorio <fog@dndg.it>"),
				new Header ("To", x),
				new Header ("Subject", "Hello"),
				new TextPart ("plain", "Just a short message to say hello!")
			)).ToList();

			Assert.AreEqual (2, msgs.Count, "Message count is wrong");
			Assert.AreEqual ("\"Federico Di Gregorio\" <fog@dndg.it>", msgs[0].From[0].ToString(), "Wrong value in From[0], message 1");
			Assert.AreEqual ("\"Federico Di Gregorio\" <fog@dndg.it>", msgs[1].From[0].ToString(), "Wrong value in From[0], message 2");
			Assert.AreEqual ("jeff@xamarin.com", msgs[0].To[0].ToString(), "Wrong value in To[0], message 1");
			Assert.AreEqual ("gg@dndg.it", msgs[1].To[0].ToString(), "Wrong value in To[0], message 2");
		}

		[Test]
		public void TestMultipartAlternative ()
		{
			var msg = new Multipart ("alternative",
				new TextPart ("plain", "Just a short message to say hello!"),
				new TextPart ("html", "<html><head></head><body><strong>Just a short message to say hello!</strong></body></html>")
			);

			Assert.AreEqual (2, msg.Count, "Parts count is wrong");
			Assert.AreEqual ("plain", msg[0].ContentType.MediaSubtype, "Parts[0] has wrong media subtype");
			Assert.AreEqual ("html", msg[1].ContentType.MediaSubtype, "Parts[1] has wrong media subtype");
			Assert.AreEqual ("Just a short message to say hello!", ((TextPart)msg[0]).Text, "Parts[0] containes wrong text");
			Assert.AreEqual ("<html><head></head><body><strong>Just a short message to say hello!</strong></body></html>", 
				((TextPart)msg[1]).Text, "Parts[1] containes wrong text");
		}

		[Test]
		public void TestMimePartContentObject ()
		{
			byte[] data = Encoding.ASCII.GetBytes ("abcd");

			// Checksum will be wrong if content is encoded in any way.
			string checksum;
			using (var md5 = MD5.Create ())
				checksum = Convert.ToBase64String (md5.ComputeHash (data));

			var msg = new MimePart ("application", "octet-stream",
				new MimeContent (new MemoryStream (data), ContentEncoding.Binary)
			);

			Assert.AreEqual (checksum, msg.ComputeContentMd5 (), "Content MD5 is wrong");
			Assert.AreEqual (ContentEncoding.Binary, msg.Content.Encoding, "ContentEncoding is wrong");
		}

		[Test]
		public void TestMimePartStream ()
		{
			byte[] data = Encoding.ASCII.GetBytes ("abcd");


			var msg = new MimePart ("application", "octet-stream",
				new MemoryStream (data)
			);

			var buffer = new MemoryStream ();
			msg.Content.DecodeTo (buffer);
			buffer.Seek (0, SeekOrigin.Begin);

			Assert.AreEqual (ContentEncoding.Default, msg.Content.Encoding, "ContentEncoding is wrong");
			Assert.AreEqual (data, buffer.ToArray (), "ContentEncoding is wrong");
		}
	}
}
