//
// MessagePartialTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2016 Xamarin Inc. (www.xamarin.com)
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
using System.Collections.Generic;

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
		public void TestArgumentExceptions ()
		{
			var message = new MimeMessage ();

			Assert.Throws<ArgumentNullException> (() => new MessagePartial (null, 1, 5));
			Assert.Throws<ArgumentOutOfRangeException> (() => new MessagePartial ("id", 0, 5));
			Assert.Throws<ArgumentOutOfRangeException> (() => new MessagePartial ("id", 6, 5));
			Assert.Throws<ArgumentNullException> (() => new MessagePartial ("id", 1, 5).Accept (null));

			Assert.Throws<ArgumentNullException> (() => MessagePartial.Split (null, 500).ToList ());
			Assert.Throws<ArgumentOutOfRangeException> (() => MessagePartial.Split (message, 0).ToList ());
		}

		[Test]
		public void TestReassemble ()
		{
			var message0 = Load (Path.Combine ("..", "..", "TestData", "partial", "message-partial.0.eml"));
			var message1 = Load (Path.Combine ("..", "..", "TestData", "partial", "message-partial.1.eml"));
			var message2 = Load (Path.Combine ("..", "..", "TestData", "partial", "message-partial.2.eml"));

			Assert.IsNotNull (message0, "Failed to parse message-partial.1.eml");
			Assert.IsNotNull (message1, "Failed to parse message-partial.1.eml");
			Assert.IsNotNull (message2, "Failed to parse message-partial.2.eml");

			Assert.IsTrue (message0.Body is MessagePartial, "The body of message-partial.0.eml is not a message/partial");
			Assert.IsTrue (message1.Body is MessagePartial, "The body of message-partial.1.eml is not a message/partial");
			Assert.IsTrue (message2.Body is MessagePartial, "The body of message-partial.2.eml is not a message/partial");

			var partials = new MessagePartial[] { (MessagePartial) message0.Body, (MessagePartial) message1.Body, (MessagePartial) message2.Body };
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join (null, partials));
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join (null));
			var message = MessagePartial.Join (partials);

			Assert.IsNotNull (message, "Failed to reconstruct the message");
			Assert.AreEqual ("Photo of a girl with feather earrings", message.Subject, "Subjects do not match");
			Assert.IsTrue (message.Body is Multipart, "Parsed message body is not a multipart");

			var multipart = (Multipart) message.Body;
			Assert.AreEqual (2, multipart.Count, "Multipart does not contain the expected number of parts");

			var part = multipart[1] as MimePart;
			Assert.IsNotNull (part, "Second part is null or not a MimePart");
			Assert.IsTrue (part.ContentType.IsMimeType ("image", "jpeg"), "Attachment is not an image/jpeg");
			Assert.AreEqual ("earrings.jpg", part.FileName, "Attachment filename is not the expected value");
		}

		[Test]
		public void TestSplit ()
		{
			var message0 = Load (Path.Combine ("..", "..", "TestData", "partial", "message-partial.0.eml"));
			var message1 = Load (Path.Combine ("..", "..", "TestData", "partial", "message-partial.1.eml"));
			var message2 = Load (Path.Combine ("..", "..", "TestData", "partial", "message-partial.2.eml"));
			var partials = new MessagePartial[] { (MessagePartial) message0.Body, (MessagePartial) message1.Body, (MessagePartial) message2.Body };
			var message = MessagePartial.Join (partials);
			var split = MessagePartial.Split (message, 1024 * 16).ToList ();
			var parts = new List<MessagePartial> ();

			Assert.AreEqual (11, split.Count, "Unexpected count");

			for (int i = 0; i < split.Count; i++) {
				parts.Add ((MessagePartial) split[i].Body);

				Assert.AreEqual (11, parts[i].Total, "Total");
				Assert.AreEqual (i + 1, parts[i].Number, "Number");
			}

			var combined = MessagePartial.Join (parts);

			using (var stream = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Unix;

				message.WriteTo (options, stream);

				var bytes0 = new byte[stream.Position];
				Array.Copy (stream.GetBuffer (), 0, bytes0, 0, (int) stream.Position);

				stream.Position = 0;

				combined.WriteTo (options, stream);

				var bytes1 = new byte[stream.Position];
				Array.Copy (stream.GetBuffer (), 0, bytes1, 0, (int) stream.Position);

				Assert.AreEqual (bytes0.Length, bytes1.Length, "bytes");

				for (int i = 0; i < bytes0.Length; i++)
					Assert.AreEqual (bytes0[i], bytes1[i], "bytes[{0}]", i);
			}
		}
	}
}
