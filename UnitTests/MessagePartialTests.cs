//
// MessagePartialTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2022 .NET Foundation and Contributors
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

			Assert.IsNull (partial.Number, "Invalid number");
			Assert.IsNull (partial.Total, "Invalid total");

			partial.ContentType.Parameters.Remove ("number");
			partial.ContentType.Parameters.Remove ("total");

			Assert.IsNull (partial.Number, "No number");
			Assert.IsNull (partial.Total, "No total");

			partial.ContentType.Parameters["number"] = "1";
			partial.ContentType.Parameters["total"] = "5";

			Assert.AreEqual (1, partial.Number.Value, "Number");
			Assert.AreEqual (5, partial.Total.Value, "Total");
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

				Assert.AreEqual (expectedBytes.Length, actualBytes.Length, "bytes");

				for (int i = 0; i < expectedBytes.Length; i++)
					Assert.AreEqual (expectedBytes[i], actualBytes[i], "bytes[{0}]", i);
			}
		}

		[Test]
		public void TestReassembleGirlOnTrainPhotoExample ()
		{
			var message0 = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "message-partial.0.eml"));
			var message1 = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "message-partial.1.eml"));
			var message2 = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "message-partial.2.eml"));
			var original = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "message-partial.eml"));

			Assert.IsNotNull (message0, "Failed to parse message-partial.0.eml");
			Assert.IsNotNull (message1, "Failed to parse message-partial.1.eml");
			Assert.IsNotNull (message2, "Failed to parse message-partial.2.eml");

			Assert.IsTrue (message0.Body is MessagePartial, "The body of message-partial.0.eml is not a message/partial");
			Assert.IsTrue (message1.Body is MessagePartial, "The body of message-partial.1.eml is not a message/partial");
			Assert.IsTrue (message2.Body is MessagePartial, "The body of message-partial.2.eml is not a message/partial");

			var partials = new MessagePartial[] { (MessagePartial) message0.Body, (MessagePartial) message1.Body, (MessagePartial) message2.Body };
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join (null, message0, partials));
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join ((MimeMessage) null, partials));
			var message = MessagePartial.Join (message0, partials);

			Assert.IsNotNull (message, "Failed to reconstruct the message");
			Assert.AreEqual ("Photo of a girl with feather earrings", message.Subject, "Subjects do not match");
			Assert.IsTrue (message.Body is Multipart, "Parsed message body is not a multipart");

			var multipart = (Multipart) message.Body;
			Assert.AreEqual (2, multipart.Count, "Multipart does not contain the expected number of parts");

			var part = multipart[1] as MimePart;
			Assert.IsNotNull (part, "Second part is null or not a MimePart");
			Assert.IsTrue (part.ContentType.IsMimeType ("image", "jpeg"), "Attachment is not an image/jpeg");
			Assert.AreEqual ("earrings.jpg", part.FileName, "Attachment filename is not the expected value");

			AssertRawMessageStreams (original, message);
		}

		[Test]
		public void TestReassembleRfc2046Example ()
		{
			var message0 = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "rfc2046.0.eml"));
			var message1 = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "rfc2046.1.eml"));
			var original = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "rfc2046.eml"));

			Assert.IsNotNull (message0, "Failed to parse rfc2046.0.eml");
			Assert.IsNotNull (message1, "Failed to parse rfc2046.1.eml");

			Assert.IsTrue (message0.Body is MessagePartial, "The body of rfc2046.0.eml is not a message/partial");
			Assert.IsTrue (message1.Body is MessagePartial, "The body of rfc2046.1.eml is not a message/partial");

			var partials = new MessagePartial[] { (MessagePartial) message0.Body, (MessagePartial) message1.Body };
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join (null, message0, partials));
			Assert.Throws<ArgumentNullException> (() => MessagePartial.Join ((MimeMessage) null, partials));
			var message = MessagePartial.Join (message0, partials);

			Assert.IsNotNull (message, "Failed to reconstruct the message");
			Assert.AreEqual ("Audio mail", message.Subject, "Subjects do not match");
			Assert.IsTrue (message.Body.ContentType.IsMimeType ("audio", "basic"), "Parsed message body is not audio/basic");

			AssertRawMessageStreams (original, message);
		}

		[Test]
		public void TestSplit ()
		{
			var message = Load (Path.Combine (TestHelper.ProjectDir, "TestData", "partial", "message-partial.eml"));
			var split = MessagePartial.Split (message, 1024 * 16).ToList ();
			var parts = new List<MessagePartial> ();

			Assert.AreEqual (11, split.Count, "Unexpected count");

			for (int i = 0; i < split.Count; i++) {
				parts.Add ((MessagePartial) split[i].Body);

				Assert.AreEqual (11, parts[i].Total, "Total");
				Assert.AreEqual (i + 1, parts[i].Number, "Number");
			}

			var combined = MessagePartial.Join (message, parts);

			AssertRawMessageStreams (message, combined);
		}
	}
}
