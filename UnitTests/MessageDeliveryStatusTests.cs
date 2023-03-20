//
// MessageDeliveryStatusTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2023 .NET Foundation and Contributors
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
	public class MessageDeliveryStatusTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var mds = new MessageDeliveryStatus ();

			Assert.Throws<ArgumentNullException> (() => mds.Accept (null));
		}

		[Test]
		public void TestStatusGroups ()
		{
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "delivery-status.txt"));

			Assert.IsInstanceOf<MultipartReport> (message.Body, "Expected top-level body part to be a multipart/report.");

			var report = (MultipartReport) message.Body;

			Assert.IsInstanceOf<MessageDeliveryStatus> (report[0], "Expected first part to be a message/delivery-status.");

			var delivery = (MessageDeliveryStatus) report[0];
			var groups = delivery.StatusGroups;

			Assert.IsNotNull (groups, "Did not expect null status groups.");
			Assert.AreEqual (2, groups.Count, "Expected 2 groups of headers.");

			Assert.AreEqual ("dns; mm1", groups[0]["Reporting-MTA"]);
			Assert.AreEqual ("Mon, 29 Jul 1996 02:12:50 -0700", groups[0]["Arrival-Date"]);

			Assert.AreEqual ("RFC822; newsletter-request@imusic.com", groups[1]["Final-Recipient"]);
			Assert.AreEqual ("failed", groups[1]["Action"]);
			Assert.AreEqual ("X-LOCAL; 500 (err.nosuchuser)", groups[1]["Diagnostic-Code"]);
		}

		// This tests issue #250
		[Test]
		public void TestStatusGroupsNoBlankLine ()
		{
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "delivery-status-no-blank-line.txt"));

			Assert.IsInstanceOf<MultipartReport> (message.Body, "Expected top-level body part to be a multipart/report.");

			var report = (MultipartReport) message.Body;

			Assert.IsInstanceOf<MessageDeliveryStatus> (report[0], "Expected first part to be a message/delivery-status.");

			var delivery = (MessageDeliveryStatus) report[0];
			var groups = delivery.StatusGroups;

			Assert.IsNotNull (groups, "Did not expect null status groups.");
			Assert.AreEqual (2, groups.Count, "Expected 2 groups of headers.");

			Assert.AreEqual ("dns; mm1", groups[0]["Reporting-MTA"]);
			Assert.AreEqual ("Mon, 29 Jul 1996 02:12:50 -0700", groups[0]["Arrival-Date"]);

			Assert.AreEqual ("RFC822; newsletter-request@imusic.com", groups[1]["Final-Recipient"]);
			Assert.AreEqual ("failed", groups[1]["Action"]);
			Assert.AreEqual ("X-LOCAL; 500 (err.nosuchuser)", groups[1]["Diagnostic-Code"]);
		}

		// This tests the bug that @alex-jitbit ran into in issue #250
		[Test]
		public void TestStatusGroupsWithContent ()
		{
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "bounce.txt"));

			Assert.IsInstanceOf<MultipartReport> (message.Body, "Expected top-level body part to be a multipart/report.");

			var report = (MultipartReport) message.Body;

			Assert.IsInstanceOf<MessageDeliveryStatus> (report[1], "Expected second part to be a message/delivery-status.");

			var delivery = (MessageDeliveryStatus) report[1];
			Assert.AreEqual ("Delivery report", delivery.ContentDescription, "ContentDescription");
			Assert.AreEqual ("934", delivery.Headers[HeaderId.ContentLength], "ContentLength");

			var groups = delivery.StatusGroups;

			Assert.IsNotNull (groups, "Did not expect null status groups.");
			Assert.AreEqual (2, groups.Count, "Expected 2 groups of headers.");

			Assert.AreEqual ("dns; hmail.jitbit.com", groups[0]["Reporting-MTA"]);
			Assert.AreEqual ("630A242E63", groups[0]["X-Postfix-Queue-ID"]);
			Assert.AreEqual ("rfc822; helpdesk@netecgc.com", groups[0]["X-Postfix-Sender"]);
			Assert.AreEqual ("Wed, 26 Jan 2022 04:06:46 -0500 (EST)", groups[0]["Arrival-Date"]);
			Assert.AreEqual ("base64", groups[0]["Content-Transfer-Encoding"]);
			Assert.AreEqual ("712", groups[0]["Content-Length"]);

			Assert.AreEqual ("rfc822; netec.test@netecgc.com", groups[1]["Final-Recipient"]);
			Assert.AreEqual ("rfc822;netec.test@netecgc.com", groups[1]["Original-Recipient"]);
			Assert.AreEqual ("failed", groups[1]["Action"]);
			Assert.AreEqual ("5.1.1", groups[1]["Status"]);
			Assert.AreEqual ("dns; https://urldefense.proofpoint.com/v2/url?u=http-3A__mx1-2Deu1.ppe-2Dhosted.com&d=DwICAQ&c=euGZstcaTDllvimEN8b7jXrwqOf-v5A_CdpgnVfiiMM&r=xGEu8UUVNHyj_BIRW7SVPK81Hnp-FSanq3-_T1am-Kg&m=RMniPmjTykiwdgbzUU7Cewy0BeD_osytuQLS6cflj30&s=0Q-rn8HZSqF10OISjAJdmdg7HT9iADG2jsaaaxtt7tE&e=", groups[1]["Remote-MTA"]);
			Assert.AreEqual ("smtp; 550 5.1.1 <netec.test@netecgc.com>: Recipient address    rejected: User unknown", groups[1]["Diagnostic-Code"]);
		}

		// This tests issue #855
		[Test]
		public void TestStatusGroupsMultipleBlankLines ()
		{
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "delivery-status-multiple-blank-lines.txt"));

			Assert.IsInstanceOf<MultipartReport> (message.Body, "Expected top-level body part to be a multipart/report.");

			var report = (MultipartReport) message.Body;

			Assert.IsInstanceOf<TextPart> (report[0], "Expected second part to be a text/plain.");
			Assert.IsInstanceOf<MessageDeliveryStatus> (report[1], "Expected second part to be a message/delivery-status.");

			var delivery = (MessageDeliveryStatus) report[1];
			var groups = delivery.StatusGroups;

			Assert.IsNotNull (groups, "Did not expect null status groups.");
			Assert.AreEqual (2, groups.Count, "Expected 2 groups of headers.");

			Assert.AreEqual ("dns;someserver.com", groups[0]["Reporting-MTA"]);

			Assert.AreEqual ("rfc822;orig_recip@customer.com", groups[1]["Original-Recipient"]);
			Assert.AreEqual ("5.5.0", groups[1]["Status"]);
			Assert.AreEqual ("smtp;550 Requested action not taken", groups[1]["Diagnostic-Code"]);
		}

		[Test]
		public void TestSerializedContent ()
		{
			const string expected = "Reporting-MTA: dns; mm1\nArrival-Date: Mon, 29 Jul 1996 02:12:50 -0700\n\nFinal-Recipient: RFC822; newsletter-request@imusic.com\nAction: failed\nDiagnostic-Code: X-LOCAL; 500 (err.nosuchuser)\n\n";
			var mds = new MessageDeliveryStatus ();
			var recipient = new HeaderList ();
			var status = new HeaderList {
				{ "Reporting-MTA", "dns; mm1" },
				{ "Arrival-Date", DateUtils.FormatDate (new DateTimeOffset (1996, 7, 29, 2, 12, 50, new TimeSpan (-7, 0, 0))) }
			};

			recipient.Add ("Final-Recipient", "RFC822; newsletter-request@imusic.com");
			recipient.Add ("Action", "failed");
			recipient.Add ("Diagnostic-Code", "X-LOCAL; 500 (err.nosuchuser)");

			mds.StatusGroups.Add (status);
			mds.StatusGroups.Add (recipient);

			Assert.IsTrue (mds.StatusGroups.Contains (status), "Expected the groups to contain the per-message status group.");
			Assert.IsTrue (mds.StatusGroups.Contains (recipient), "Expected the groups to contain the recipient status group.");
			Assert.IsFalse (mds.StatusGroups.IsReadOnly, "The status groups should not be read-only.");

			using (var memory = new MemoryStream ()) {
				mds.Content.DecodeTo (memory);

				var text = Encoding.ASCII.GetString (memory.GetBuffer (), 0, (int) memory.Length).Replace ("\r\n", "\n");
				Assert.AreEqual (expected, text);
			}

			var dummy = new HeaderList {
				{ "Dummy-Header", "dummy value" }
			};

			mds.StatusGroups.Add (dummy);

			Assert.IsTrue (mds.StatusGroups.Contains (dummy), "Expected the groups to contain the dummy group.");
			Assert.IsTrue (mds.StatusGroups.Remove (dummy), "Expected removal of the dummy group to be successful.");

			var expectedContent = mds.Content;

			dummy.Add ("Bogus-Header", "bogus value");

			Assert.AreEqual (expectedContent, mds.Content, "The content should not have changed since the dummy group has been removed.");

			mds.StatusGroups.Clear ();

			using (var memory = new MemoryStream ()) {
				mds.Content.DecodeTo (memory);

				var text = Encoding.ASCII.GetString (memory.GetBuffer (), 0, (int) memory.Length).Replace ("\r\n", "\n");

				Assert.AreEqual (string.Empty, text);
			}
		}
	}
}
