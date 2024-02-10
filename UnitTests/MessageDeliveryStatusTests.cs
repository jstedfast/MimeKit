//
// MessageDeliveryStatusTests.cs
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

			Assert.That (message.Body, Is.InstanceOf<MultipartReport> (), "Expected top-level body part to be a multipart/report.");

			var report = (MultipartReport) message.Body;

			Assert.That (report[0], Is.InstanceOf<MessageDeliveryStatus> (), "Expected first part to be a message/delivery-status.");

			var delivery = (MessageDeliveryStatus) report[0];
			var groups = delivery.StatusGroups;

			Assert.That (groups, Is.Not.Null, "Did not expect null status groups.");
			Assert.That (groups.Count, Is.EqualTo (2), "Expected 2 groups of headers.");

			Assert.That (groups[0]["Reporting-MTA"], Is.EqualTo ("dns; mm1"));
			Assert.That (groups[0]["Arrival-Date"], Is.EqualTo ("Mon, 29 Jul 1996 02:12:50 -0700"));

			Assert.That (groups[1]["Final-Recipient"], Is.EqualTo ("RFC822; newsletter-request@imusic.com"));
			Assert.That (groups[1]["Action"], Is.EqualTo ("failed"));
			Assert.That (groups[1]["Diagnostic-Code"], Is.EqualTo ("X-LOCAL; 500 (err.nosuchuser)"));
		}

		// This tests issue #250
		[Test]
		public void TestStatusGroupsNoBlankLine ()
		{
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "delivery-status-no-blank-line.txt"));

			Assert.That (message.Body, Is.InstanceOf<MultipartReport> (), "Expected top-level body part to be a multipart/report.");

			var report = (MultipartReport) message.Body;

			Assert.That (report[0], Is.InstanceOf<MessageDeliveryStatus> (), "Expected first part to be a message/delivery-status.");

			var delivery = (MessageDeliveryStatus) report[0];
			var groups = delivery.StatusGroups;

			Assert.That (groups, Is.Not.Null, "Did not expect null status groups.");
			Assert.That (groups.Count, Is.EqualTo (2), "Expected 2 groups of headers.");

			Assert.That (groups[0]["Reporting-MTA"], Is.EqualTo ("dns; mm1"));
			Assert.That (groups[0]["Arrival-Date"], Is.EqualTo ("Mon, 29 Jul 1996 02:12:50 -0700"));

			Assert.That (groups[1]["Final-Recipient"], Is.EqualTo ("RFC822; newsletter-request@imusic.com"));
			Assert.That (groups[1]["Action"], Is.EqualTo ("failed"));
			Assert.That (groups[1]["Diagnostic-Code"], Is.EqualTo ("X-LOCAL; 500 (err.nosuchuser)"));
		}

		// This tests the bug that @alex-jitbit ran into in issue #250
		[Test]
		public void TestStatusGroupsWithContent ()
		{
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "bounce.txt"));

			Assert.That (message.Body, Is.InstanceOf<MultipartReport> (), "Expected top-level body part to be a multipart/report.");

			var report = (MultipartReport) message.Body;

			Assert.That (report[1], Is.InstanceOf<MessageDeliveryStatus> (), "Expected second part to be a message/delivery-status.");

			var delivery = (MessageDeliveryStatus) report[1];
			Assert.That (delivery.ContentDescription, Is.EqualTo ("Delivery report"), "ContentDescription");
			Assert.That (delivery.Headers[HeaderId.ContentLength], Is.EqualTo ("934"), "ContentLength");

			var groups = delivery.StatusGroups;

			Assert.That (groups, Is.Not.Null, "Did not expect null status groups.");
			Assert.That (groups.Count, Is.EqualTo (2), "Expected 2 groups of headers.");

			Assert.That (groups[0]["Reporting-MTA"], Is.EqualTo ("dns; hmail.jitbit.com"));
			Assert.That (groups[0]["X-Postfix-Queue-ID"], Is.EqualTo ("630A242E63"));
			Assert.That (groups[0]["X-Postfix-Sender"], Is.EqualTo ("rfc822; helpdesk@netecgc.com"));
			Assert.That (groups[0]["Arrival-Date"], Is.EqualTo ("Wed, 26 Jan 2022 04:06:46 -0500 (EST)"));
			Assert.That (groups[0]["Content-Transfer-Encoding"], Is.EqualTo ("base64"));
			Assert.That (groups[0]["Content-Length"], Is.EqualTo ("712"));

			Assert.That (groups[1]["Final-Recipient"], Is.EqualTo ("rfc822; netec.test@netecgc.com"));
			Assert.That (groups[1]["Original-Recipient"], Is.EqualTo ("rfc822;netec.test@netecgc.com"));
			Assert.That (groups[1]["Action"], Is.EqualTo ("failed"));
			Assert.That (groups[1]["Status"], Is.EqualTo ("5.1.1"));
			Assert.That (groups[1]["Remote-MTA"], Is.EqualTo ("dns; https://urldefense.proofpoint.com/v2/url?u=http-3A__mx1-2Deu1.ppe-2Dhosted.com&d=DwICAQ&c=euGZstcaTDllvimEN8b7jXrwqOf-v5A_CdpgnVfiiMM&r=xGEu8UUVNHyj_BIRW7SVPK81Hnp-FSanq3-_T1am-Kg&m=RMniPmjTykiwdgbzUU7Cewy0BeD_osytuQLS6cflj30&s=0Q-rn8HZSqF10OISjAJdmdg7HT9iADG2jsaaaxtt7tE&e="));
			Assert.That (groups[1]["Diagnostic-Code"], Is.EqualTo ("smtp; 550 5.1.1 <netec.test@netecgc.com>: Recipient address    rejected: User unknown"));
		}

		// This tests issue #855
		[Test]
		public void TestStatusGroupsMultipleBlankLines ()
		{
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "delivery-status-multiple-blank-lines.txt"));

			Assert.That (message.Body, Is.InstanceOf<MultipartReport> (), "Expected top-level body part to be a multipart/report.");

			var report = (MultipartReport) message.Body;

			Assert.That (report[0], Is.InstanceOf<TextPart> (), "Expected second part to be a text/plain.");
			Assert.That (report[1], Is.InstanceOf<MessageDeliveryStatus> (), "Expected second part to be a message/delivery-status.");

			var delivery = (MessageDeliveryStatus) report[1];
			var groups = delivery.StatusGroups;

			Assert.That (groups, Is.Not.Null, "Did not expect null status groups.");
			Assert.That (groups.Count, Is.EqualTo (2), "Expected 2 groups of headers.");

			Assert.That (groups[0]["Reporting-MTA"], Is.EqualTo ("dns;someserver.com"));

			Assert.That (groups[1]["Original-Recipient"], Is.EqualTo ("rfc822;orig_recip@customer.com"));
			Assert.That (groups[1]["Status"], Is.EqualTo ("5.5.0"));
			Assert.That (groups[1]["Diagnostic-Code"], Is.EqualTo ("smtp;550 Requested action not taken"));
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

			Assert.That (mds.StatusGroups.Contains (status), Is.True, "Expected the groups to contain the per-message status group.");
			Assert.That (mds.StatusGroups.Contains (recipient), Is.True, "Expected the groups to contain the recipient status group.");
			Assert.That (mds.StatusGroups.IsReadOnly, Is.False, "The status groups should not be read-only.");

			using (var memory = new MemoryStream ()) {
				mds.Content.DecodeTo (memory);

				var text = Encoding.ASCII.GetString (memory.GetBuffer (), 0, (int) memory.Length).Replace ("\r\n", "\n");
				Assert.That (text, Is.EqualTo (expected));
			}

			var dummy = new HeaderList {
				{ "Dummy-Header", "dummy value" }
			};

			mds.StatusGroups.Add (dummy);

			Assert.That (mds.StatusGroups.Contains (dummy), Is.True, "Expected the groups to contain the dummy group.");
			Assert.That (mds.StatusGroups.Remove (dummy), Is.True, "Expected removal of the dummy group to be successful.");

			var expectedContent = mds.Content;

			dummy.Add ("Bogus-Header", "bogus value");

			Assert.That (mds.Content, Is.EqualTo (expectedContent), "The content should not have changed since the dummy group has been removed.");

			mds.StatusGroups.Clear ();

			using (var memory = new MemoryStream ()) {
				mds.Content.DecodeTo (memory);

				var text = Encoding.ASCII.GetString (memory.GetBuffer (), 0, (int) memory.Length).Replace ("\r\n", "\n");

				Assert.That (text, Is.EqualTo (string.Empty));
			}
		}
	}
}
