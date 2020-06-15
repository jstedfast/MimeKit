//
// MessageDeliveryStatusTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
using System.Text;

using NUnit.Framework;

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
		public void TestMimeParser ()
		{
			var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "delivery-status.txt"));

			Assert.IsInstanceOf<Multipart> (message.Body, "Expected top-level body part to be a multipart/report.");

			var multipart = (Multipart) message.Body;

			Assert.IsInstanceOf<MessageDeliveryStatus> (multipart[0], "Expected first part to be a message/delivery-status.");

			var mds = (MessageDeliveryStatus) multipart[0];
			var groups = mds.StatusGroups;

			Assert.IsNotNull (groups, "Did not expect null status groups.");
			Assert.AreEqual (2, groups.Count, "Expected 2 groups of headers.");

			Assert.AreEqual ("dns; mm1", groups[0]["Reporting-MTA"]);
			Assert.AreEqual ("Mon, 29 Jul 1996 02:12:50 -0700", groups[0]["Arrival-Date"]);

			Assert.AreEqual ("RFC822; newsletter-request@imusic.com", groups[1]["Final-Recipient"]);
			Assert.AreEqual ("failed", groups[1]["Action"]);
			Assert.AreEqual ("X-LOCAL; 500 (err.nosuchuser)", groups[1]["Diagnostic-Code"]);
		}

		[Test]
		public void TestSerializedContent ()
		{
			const string expected = "Reporting-MTA: dns; mm1\nArrival-Date: Mon, 29 Jul 1996 02:12:50 -0700\n\nFinal-Recipient: RFC822; newsletter-request@imusic.com\nAction: failed\nDiagnostic-Code: X-LOCAL; 500 (err.nosuchuser)\n\n";
			var mds = new MessageDeliveryStatus ();
			var recipient = new HeaderList ();
			var status = new HeaderList ();

			status.Add ("Reporting-MTA", "dns; mm1");
			status.Add ("Arrival-Date", DateUtils.FormatDate (new DateTimeOffset (1996, 7, 29, 2, 12, 50, new TimeSpan (-7, 0, 0))));

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

			var dummy = new HeaderList ();
			dummy.Add ("Dummy-Header", "dummy value");

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
