//
// MessageFeedbackReportTests.cs
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

using System.Text;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MessageFeedbackReportTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var mfr = new MessageFeedbackReport ();

			Assert.Throws<ArgumentNullException> (() => mfr.Accept (null));
		}

		[Test]
		public void TestMimeParser ()
		{
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "feedback-report.txt"));

			Assert.IsInstanceOf<MultipartReport> (message.Body, "Expected top-level body part to be a multipart/report.");

			var multipart = (MultipartReport) message.Body;

			Assert.IsInstanceOf<MessageFeedbackReport> (multipart[1], "Expected second part to be a message/feedback-report.");
			Assert.AreEqual ("feedback-report", multipart.ReportType);

			var mfr = (MessageFeedbackReport) multipart[1];
			var fields = mfr.Fields;

			Assert.IsNotNull (fields, "Did not expect null set of fields.");
			Assert.AreEqual (12, fields.Count, "Expected 12 fields.");

			Assert.AreEqual ("abuse", fields["Feedback-Type"]);
			Assert.AreEqual ("SomeGenerator/1.0", fields["User-Agent"]);
			Assert.AreEqual ("1", fields["Version"]);
			Assert.AreEqual ("<somespammer@example.net>", fields["Original-Mail-From"]);
			Assert.AreEqual ("<user@example.com>", fields["Original-Rcpt-To"]);
			Assert.AreEqual ("Thu, 8 Mar 2005 14:00:00 EDT", fields["Received-Date"]);
			Assert.AreEqual ("192.0.2.2", fields["Source-IP"]);
			Assert.AreEqual ("mail.example.com;               spf=fail smtp.mail=somespammer@example.com", fields["Authentication-Results"]);
			Assert.AreEqual ("example.net", fields["Reported-Domain"]);
			Assert.AreEqual ("http://example.net/earn_money.html", fields["Reported-Uri"]);
			//Assert.AreEqual ("mailto:user@example.com", fields["Reported-Uri"]);
			Assert.AreEqual ("user@example.com", fields["Removal-Recipient"]);
		}

		[Test]
		public void TestSerializedContent ()
		{
			const string expected = "Feedback-Type: abuse\nUser-Agent: SomeGenerator/1.0\nVersion: 1\n\n";
			var mfr = new MessageFeedbackReport ();

			mfr.Fields.Add ("Feedback-Type", "abuse");
			mfr.Fields.Add ("User-Agent", "SomeGenerator/1.0");
			mfr.Fields.Add ("Version", "1");

			using (var memory = new MemoryStream ()) {
				mfr.Content.DecodeTo (memory);

				var text = Encoding.ASCII.GetString (memory.GetBuffer (), 0, (int) memory.Length).Replace ("\r\n", "\n");
				Assert.AreEqual (expected, text);
			}
		}
	}
}
