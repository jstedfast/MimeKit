//
// MimeMessageTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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
using System.Net.Mail;

using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MimeMessageTests
	{
		[Test]
		public void TestReserialization ()
		{
			const string rawMessageText = @"X-Andrew-Authenticated-As: 4099;greenbush.galaxy;Nathaniel Borenstein
Received: from Messages.8.5.N.CUILIB.3.45.SNAP.NOT.LINKED.greenbush.galaxy.sun4.41
          via MS.5.6.greenbush.galaxy.sun4_41;
          Fri, 12 Jun 1992 13:29:05 -0400 (EDT)
Message-ID : <UeCBvVq0M2Yt4oUA83@thumper.bellcore.com>
Date: Fri, 12 Jun 1992 13:29:05 -0400 (EDT)
From: Nathaniel Borenstein <nsb>
X-Andrew-Message-Size: 152+1
MIME-Version: 1.0
Content-Type: multipart/alternative; 
	boundary=""Interpart.Boundary.IeCBvV20M2YtEoUA0A""
To: Ned Freed <ned@innosoft.com>,
    ysato@etl.go.jp (Yutaka Sato =?ISO-2022-JP?B?GyRAOjRGI0stGyhK?= )
Subject: MIME & int'l mail

> THIS IS A MESSAGE IN 'MIME' FORMAT.  Your mail reader does not support MIME.
> Please read the first section, which is plain text, and ignore the rest.

--Interpart.Boundary.IeCBvV20M2YtEoUA0A
Content-type: text/plain; charset=US-ASCII

In honor of the Communications Week error about MIME's ability to handle
international character sets. a screen dump:

[An Andrew ToolKit view (mailobjv) was included here, but could not be
displayed.]
Just for fun....  -- Nathaniel

--Interpart.Boundary.IeCBvV20M2YtEoUA0A
Content-Type: multipart/mixed; 
	boundary=""Alternative.Boundary.IeCBvV20M2Yt4oU=wd""

--Alternative.Boundary.IeCBvV20M2Yt4oU=wd
Content-type: text/richtext; charset=US-ASCII
Content-Transfer-Encoding: quoted-printable

In honor of the <italic>Communications Week</italic> error about MIME's abilit=
y to handle international character sets. a screen dump:<nl>
<nl>

--Alternative.Boundary.IeCBvV20M2Yt4oU=wd
Content-type: image/gif
Content-Description: Some international characters
Content-Transfer-Encoding: base64

R0lGODdhEgLiAKEAAAAAAP///wAA////4CwAAAAAEgLiAAAC/oSPqcvtD6OctNqLs968
...
R+mUIAiVUTmCU0mVJmiVV5mCfaiVQtaUXVlKXwmWZiSWY3lDZWmWIISWaalUWcmW+bWW
b9lAcSmXCUSXdWlKbomX7HWXe4llXOmXQAmYgTmUg0mYRmmYh5mUscGYjemYjwmZkSmZ
k0mZlWmZl4mZqVEAADs=

--Alternative.Boundary.IeCBvV20M2Yt4oU=wd
Content-type: text/richtext; charset=US-ASCII
Content-Transfer-Encoding: quoted-printable

<nl>
<nl>
Just for fun....  -- Nathaniel<nl>

--Alternative.Boundary.IeCBvV20M2Yt4oU=wd--

--Interpart.Boundary.IeCBvV20M2YtEoUA0A--
";
			string result;

			using (var source = new MemoryStream (Encoding.UTF8.GetBytes (rawMessageText))) {
				var parser = new MimeParser (source, MimeFormat.Default);
				var message = parser.ParseMessage ();

				using (var serialized = new MemoryStream ()) {
					message.WriteTo (serialized);

					result = Encoding.UTF8.GetString (serialized.ToArray ());
				}
			}

			Assert.AreEqual (rawMessageText, result, "Reserialized message is not identical to the original.");
		}

		[Test]
		public void TestMailMessageToMimeMessage ()
		{
			var mail = new MailMessage ();
			mail.Body = null;

			var text = new MemoryStream (Encoding.ASCII.GetBytes ("This is plain text."), false);
			mail.AlternateViews.Add (new AlternateView (text, "text/plain"));

			var html = new MemoryStream (Encoding.ASCII.GetBytes ("This is HTML."), false);
			mail.AlternateViews.Add (new AlternateView (html, "text/html"));

			var message = (MimeMessage) mail;

			Assert.IsTrue (message.Body is Multipart, "THe top-level MIME part should be a multipart.");
			Assert.IsTrue (message.Body.ContentType.Matches ("multipart", "alternative"), "The top-level MIME part should be multipart/alternative.");

			var multipart = (Multipart) message.Body;

			Assert.AreEqual (2, multipart.Count, "Expected 2 MIME parts within the multipart/alternative.");
		}

		[Test]
		public void TestIssue135 ()
		{
			var message = new MimeMessage ();
			message.Body = new TextPart ("plain") {
				ContentTransferEncoding = ContentEncoding.Base64,
				ContentObject = new ContentObject (new MemoryStream (new byte[1], false))
			};

			try {
				message.ToString ();
			} catch (Exception ex) {
				Assert.Fail (ex.Message);
			}
		}
	}
}
