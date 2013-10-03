//
// AssortedTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
using System.Collections.Generic;
using System.Linq;
using System.IO;

using NUnit.Framework;

using MimeKit;
using MimeKit.Utils;

namespace UnitTests {
	[TestFixture]
	public class ConstructorTests
	{
		[Test]
		public void TestMimeMessageWithHeaders ()
		{
			var msg = new MimeMessage (
				new Header ("From", "Federico Di Gregorio <fog@dndg.it>"),
				new Header ("To", "jeff@xamarin.com"),
				new Header[] { new Header ("Cc", "fog@dndg.it"), new Header ("Cc", "<gg@dndg.it>") },
				new Header ("Subject", "Hello"),
				new TextPart ("plain", "Just a short message to say hello!")
			);

			Assert.AreEqual (1, msg.From.Count, "Wrong count in From");
			Assert.AreEqual ("\"Federico Di Gregorio\" <fog@dndg.it>", msg.From[0].ToString(), "Wrong value in From[0]");
			Assert.AreEqual (1, msg.To.Count, "Wrong count in To");
			Assert.AreEqual ("jeff@xamarin.com", msg.To[0].ToString(), "Wrong value in To[0]");
			Assert.AreEqual (2, msg.Cc.Count, 2, "Wrong count in Cc");
			Assert.AreEqual ("fog@dndg.it", msg.Cc[0].ToString(), "Wrong value in Cc[0]");
			Assert.AreEqual ("gg@dndg.it", msg.Cc[1].ToString(), "Wrong value in Cc[1]");
			Assert.AreEqual ("Hello", msg.Subject, "Wrong value in Subject");		
		}

		[Test]
		public void TestGenerateMultipleMessagesWithLinq ()
		{
			string[] destinations = new string[] { "jeff@xamarin.com", "gg@dndg.it" };

			IList<MimeMessage> msgs = destinations.Select(x => new MimeMessage(
				new Header ("From", "Federico Di Gregorio <fog@dndg.it>"),
				new Header ("To", x),
				new Header ("Subject", "Hello"),
				new TextPart ("plain", "Just a short message to say hello!")
			)).ToList();

			Assert.AreEqual (2, msgs.Count, "Message count is wrong");
			Assert.AreEqual ("\"Federico Di Gregorio\" <fog@dndg.it>", msgs[0].From[0].ToString(), "Wrong value in From[0], message 1");
			Assert.AreEqual ("\"Federico Di Gregorio\" <fog@dndg.it>", msgs[1].From[0].ToString(), "Wrong value in From[0], message 2");
			Assert.AreEqual ("jeff@xamarin.com", msgs[0].To[0].ToString(), "Wrong value in To[0], message 1");
			Assert.AreEqual ("gg@dndg.it", msgs[1].To[0].ToString(), "Wrong value in To[0], message 2");
		}
	}
}
