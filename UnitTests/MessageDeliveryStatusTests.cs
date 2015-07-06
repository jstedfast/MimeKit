//
// MessageDeliveryStatusTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MessageDeliveryStatusTests
	{
		[Test]
		public void TestParser ()
		{
			var message = MimeMessage.Load (Path.Combine ("..", "..", "TestData", "messages", "multipart-report.txt"));

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
	}
}
