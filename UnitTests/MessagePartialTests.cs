//
// MessagePartialTests.cs
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
			var partials = new List<MessagePartial> ();
			var message = new MimeMessage ();

			partials.Add (new MessagePartial ("abc@example.com", 1, 5) {
				Content = new MimeContent (new MemoryStream (Array.Empty<byte> (), false))
			});
			partials.Add (new MessagePartial ("abc@example.com", 2, 5) {
				Content = new MimeContent (new MemoryStream (Array.Empty<byte> (), false))
			});
			partials.Add (new MessagePartial ("abc@example.com", 3, 5) {
				Content = new MimeContent (new MemoryStream (Array.Empty<byte> (), false))
			});
			partials.Add (new MessagePartial ("abc@example.com", 4, 5) {
				Content = new MimeContent (new MemoryStream (Array.Empty<byte> (), false))
			});
			partials.Add (new MessagePartial ("abc@example.com", 5, 5) {
				Content = new MimeContent (new MemoryStream (Array.Empty<byte> (), false))
			});

			Assert.Throws<ArgumentNullException> (() => new MessagePartial (null, 1, 5));
			Assert.Throws<ArgumentOutOfRangeException> (() => new MessagePartial ("id", 0, 5));
			Assert.Throws<ArgumentOutOfRangeException> (() => new MessagePartial ("id", 6, 5));
			Assert.Throws<ArgumentNullException> (() => new MessagePartial ("id", 1, 5).Accept (null));

			Assert.Throws<ArgumentNullException> (() => MessagePartial.Split (null, 500).ToList ());
			Assert.Throws<ArgumentOutOfRangeException> (() => MessagePartial.Split (message, 0).ToList ());

			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join (null, partials));
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join (message, null));

			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join (null, message, partials));
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join (ParserOptions.Default, null, partials));
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join (ParserOptions.Default, message, null));

			partials[4].ContentType.Parameters.Remove ("total");
			Assert.Throws<ArgumentException> (() => MessagePartial.Join (message, partials));

			partials.RemoveAt (4);
			Assert.Throws<ArgumentException> (() => MessagePartial.Join (message, partials));

			partials.Add (new MessagePartial ("abc@example.com", 5, 5) {
				Content = new MimeContent (new MemoryStream (Array.Empty<byte> (), false))
			});
			partials[3].ContentType.Parameters.Remove ("number");
			partials[3].ContentType.Parameters.Add ("number", "1"); // this should leave us with a duplicate
			Assert.Throws<ArgumentException> (() => MessagePartial.Join (message, partials));

			partials[3].ContentType.Parameters["number"] = "4";
			partials[3].ContentType.Parameters["id"] = "xyz@example.com"; // mismatch identifiers
			Assert.Throws<InvalidOperationException> (() => MessagePartial.Join (message, partials));

			partials[3].ContentType.Parameters["id"] = "abc@example.com";
			partials[3].ContentType.Parameters.Remove ("number"); // missing number
			Assert.Throws<InvalidOperationException> (() => MessagePartial.Join (message, partials));
		}

		[Test]
		public void TestNumberAndTotalParameters ()
		{
			var partial = new MessagePartial ("abc@example.com", 1, 5);
			partial.ContentType.Parameters["number"] = "invalid";
			partial.ContentType.Parameters["total"] = "invalid";

			Assert.That (partial.Number, Is.Null, "Invalid number");
			Assert.That (partial.Total, Is.Null, "Invalid total");

			partial.ContentType.Parameters.Remove ("number");
			partial.ContentType.Parameters.Remove ("total");

			Assert.That (partial.Number, Is.Null, "No number");
			Assert.That (partial.Total, Is.Null, "No total");

			partial.ContentType.Parameters["number"] = "1";
			partial.ContentType.Parameters["total"] = "5";

			Assert.That (partial.Number.Value, Is.EqualTo (1), "Number");
			Assert.That (partial.Total.Value, Is.EqualTo (5), "Total");
		}

		static void AssertRawMessageStreams (MimeMessage expected, MimeMessage actual)
		{
			using (var stream = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Unix;

				expected.WriteTo (options, stream);

				var expectedBytes = new byte[stream.Position];
				Array.Copy (stream.GetBuffer (), 0, expectedBytes, 0, (int) stream.Position);

				stream.Position = 0;

				actual.WriteTo (options, stream);

				var actualBytes = new byte[stream.Position];
				Array.Copy (stream.GetBuffer (), 0, actualBytes, 0, (int) stream.Position);

				Assert.That (actualBytes.Length, Is.EqualTo (expectedBytes.Length), "bytes");

				for (int i = 0; i < expectedBytes.Length; i++)
					Assert.That (actualBytes[i], Is.EqualTo (expectedBytes[i]), $"bytes[{i}]");
			}
		}

		[Test]
		public void TestReassembleGirlOnTrainPhotoExample ()
		{
			var message0 = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "message-partial.0.eml"));
			var message1 = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "message-partial.1.eml"));
			var message2 = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "message-partial.2.eml"));
			var original = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "message-partial.eml"));

			Assert.That (message0, Is.Not.Null, "Failed to parse message-partial.0.eml");
			Assert.That (message1, Is.Not.Null, "Failed to parse message-partial.1.eml");
			Assert.That (message2, Is.Not.Null, "Failed to parse message-partial.2.eml");

			Assert.That (message0.Body is MessagePartial, Is.True, "The body of message-partial.0.eml is not a message/partial");
			Assert.That (message1.Body is MessagePartial, Is.True, "The body of message-partial.1.eml is not a message/partial");
			Assert.That (message2.Body is MessagePartial, Is.True, "The body of message-partial.2.eml is not a message/partial");

			var partials = new MessagePartial[] { (MessagePartial) message0.Body, (MessagePartial) message1.Body, (MessagePartial) message2.Body };
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join (null, message0, partials));
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join ((MimeMessage) null, partials));
			var message = MessagePartial.Join (message0, partials);

			Assert.That (message, Is.Not.Null, "Failed to reconstruct the message");
			Assert.That (message.Subject, Is.EqualTo ("Photo of a girl with feather earrings"), "Subjects do not match");
			Assert.That (message.Body is Multipart, Is.True, "Parsed message body is not a multipart");

			var multipart = (Multipart) message.Body;
			Assert.That (multipart.Count, Is.EqualTo (2), "Multipart does not contain the expected number of parts");

			var part = multipart[1] as MimePart;
			Assert.That (part, Is.Not.Null, "Second part is null or not a MimePart");
			Assert.That (part.ContentType.IsMimeType ("image", "jpeg"), Is.True, "Attachment is not an image/jpeg");
			Assert.That (part.FileName, Is.EqualTo ("earrings.jpg"), "Attachment filename is not the expected value");

			AssertRawMessageStreams (original, message);
		}

		[Test]
		public void TestReassembleRfc2046Example ()
		{
			var message0 = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "rfc2046.0.eml"));
			var message1 = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "rfc2046.1.eml"));
			var original = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "rfc2046.eml"));

			Assert.That (message0, Is.Not.Null, "Failed to parse rfc2046.0.eml");
			Assert.That (message1, Is.Not.Null, "Failed to parse rfc2046.1.eml");

			Assert.That (message0.Body is MessagePartial, Is.True, "The body of rfc2046.0.eml is not a message/partial");
			Assert.That (message1.Body is MessagePartial, Is.True, "The body of rfc2046.1.eml is not a message/partial");

			var partials = new MessagePartial[] { (MessagePartial) message0.Body, (MessagePartial) message1.Body };
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join (null, message0, partials));
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join ((MimeMessage) null, partials));
			var message = MessagePartial.Join (message0, partials);

			Assert.That (message, Is.Not.Null, "Failed to reconstruct the message");
			Assert.That (message.Subject, Is.EqualTo ("Audio mail"), "Subjects do not match");
			Assert.That (message.Body.ContentType.IsMimeType ("audio", "basic"), Is.True, "Parsed message body is not audio/basic");

			AssertRawMessageStreams (original, message);
		}

		[Test]
		public void TestSplit ()
		{
			var message = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "message-partial.eml"));
			var split = MessagePartial.Split (message, 1024 * 16).ToList ();
			var parts = new List<MessagePartial> ();

			Assert.That (split.Count, Is.EqualTo (11), "Unexpected count");

			for (int i = 0; i < split.Count; i++) {
				parts.Add ((MessagePartial) split[i].Body);

				Assert.That (parts[i].Total, Is.EqualTo (11), "Total");
				Assert.That (parts[i].Number, Is.EqualTo (i + 1), "Number");
			}

			var combined = MessagePartial.Join (message, parts);

			AssertRawMessageStreams (message, combined);
		}
	}
}
