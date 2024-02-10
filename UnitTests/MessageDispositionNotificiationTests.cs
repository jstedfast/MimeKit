//
// MessageDispositionNotificiationTests.cs
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
	public class MessageDispositionNotificiationTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var mdn = new MessageDispositionNotification ();

			Assert.Throws<ArgumentNullException> (() => mdn.Accept (null));
		}

		[Test]
		public void TestMimeParser ()
		{
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "disposition-notification.txt"));

			Assert.That (message.Body, Is.InstanceOf<MultipartReport> (), "Expected top-level body part to be a multipart/report.");

			var multipart = (MultipartReport) message.Body;

			Assert.That (multipart[1], Is.InstanceOf<MessageDispositionNotification> (), "Expected second part to be a message/disposition-notification.");
			Assert.That (multipart.ReportType, Is.EqualTo ("disposition-notification"));

			var mdn = (MessageDispositionNotification) multipart[1];
			var fields = mdn.Fields;

			Assert.That (fields, Is.Not.Null, "Did not expect null set of fields.");
			Assert.That (fields.Count, Is.EqualTo (5), "Expected 5 fields.");

			Assert.That (fields["Reporting-UA"], Is.EqualTo ("joes-pc.cs.example.com; Foomail 97.1"));
			Assert.That (fields["Original-Recipient"], Is.EqualTo ("rfc822;Joe_Recipient@example.com"));
			Assert.That (fields["Final-Recipient"], Is.EqualTo ("rfc822;Joe_Recipient@example.com"));
			Assert.That (fields["Original-Message-Id"], Is.EqualTo ("<199509192301.23456@example.org>"));
			Assert.That (fields["Disposition"], Is.EqualTo ("manual-action/MDN-sent-manually; displayed"));
		}

		[Test]
		public void TestSerializedContent ()
		{
			const string expected = "Reporting-UA: joes-pc.cs.example.com; Foomail 97.1\nOriginal-Recipient: rfc822;Joe_Recipient@example.com\nFinal-Recipient: rfc822;Joe_Recipient@example.com\nOriginal-Message-ID: <199509192301.23456@example.org>\nDisposition: manual-action/MDN-sent-manually; displayed\n\n";
			var mdn = new MessageDispositionNotification ();

			mdn.Fields.Add ("Reporting-UA", "joes-pc.cs.example.com; Foomail 97.1");
			mdn.Fields.Add ("Original-Recipient", "rfc822;Joe_Recipient@example.com");
			mdn.Fields.Add ("Final-Recipient", "rfc822;Joe_Recipient@example.com");
			mdn.Fields.Add ("Original-Message-ID", "<199509192301.23456@example.org>");
			mdn.Fields.Add ("Disposition", "manual-action/MDN-sent-manually; displayed");

			using (var memory = new MemoryStream ()) {
				mdn.Content.DecodeTo (memory);

				var text = Encoding.ASCII.GetString (memory.GetBuffer (), 0, (int) memory.Length).Replace ("\r\n", "\n");
				Assert.That (text, Is.EqualTo (expected));
			}
		}
	}
}
