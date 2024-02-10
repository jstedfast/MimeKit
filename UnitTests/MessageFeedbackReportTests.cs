//
// MessageFeedbackReportTests.cs
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

			Assert.That (message.Body, Is.InstanceOf<MultipartReport> (), "Expected top-level body part to be a multipart/report.");

			var multipart = (MultipartReport) message.Body;

			Assert.That (multipart[1], Is.InstanceOf<MessageFeedbackReport> (), "Expected second part to be a message/feedback-report.");
			Assert.That (multipart.ReportType, Is.EqualTo ("feedback-report"));

			var mfr = (MessageFeedbackReport) multipart[1];
			var fields = mfr.Fields;

			Assert.That (fields, Is.Not.Null, "Did not expect null set of fields.");
			Assert.That (fields.Count, Is.EqualTo (12), "Expected 12 fields.");

			Assert.That (fields["Feedback-Type"], Is.EqualTo ("abuse"));
			Assert.That (fields["User-Agent"], Is.EqualTo ("SomeGenerator/1.0"));
			Assert.That (fields["Version"], Is.EqualTo ("1"));
			Assert.That (fields["Original-Mail-From"], Is.EqualTo ("<somespammer@example.net>"));
			Assert.That (fields["Original-Rcpt-To"], Is.EqualTo ("<user@example.com>"));
			Assert.That (fields["Received-Date"], Is.EqualTo ("Thu, 8 Mar 2005 14:00:00 EDT"));
			Assert.That (fields["Source-IP"], Is.EqualTo ("192.0.2.2"));
			Assert.That (fields["Authentication-Results"], Is.EqualTo ("mail.example.com;               spf=fail smtp.mail=somespammer@example.com"));
			Assert.That (fields["Reported-Domain"], Is.EqualTo ("example.net"));
			Assert.That (fields["Reported-Uri"], Is.EqualTo ("http://example.net/earn_money.html"));
			//Assert.That (fields["Reported-Uri"], Is.EqualTo ("mailto:user@example.com"));
			Assert.That (fields["Removal-Recipient"], Is.EqualTo ("user@example.com"));
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
				Assert.That (text, Is.EqualTo (expected));
			}
		}
	}
}
