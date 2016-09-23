//
// MimePartTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2016 Jeffrey Stedfast
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

using NUnit.Framework;

using MimeKit;
using MimeKit.Utils;

namespace UnitTests
{
	[TestFixture]
	public class MimePartTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var part = new MimePart ();

			Assert.Throws<ArgumentNullException> (() => new MimePart ((string) null));
			Assert.Throws<ArgumentNullException> (() => new MimePart ((ContentType) null));
			Assert.Throws<ArgumentNullException> (() => new MimePart (null, "octet-stream"));
			Assert.Throws<ArgumentNullException> (() => new MimePart ("application", null));
			Assert.Throws<ArgumentNullException> (() => part.Accept (null));

			Assert.Throws<ArgumentOutOfRangeException> (() => part.ContentDuration = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => part.Prepare (EncodingConstraint.SevenBit, 1));
		}

		[Test]
		public void TestContentBase ()
		{
			var part = new MimePart ();
			var uri = new Uri ("http://www.google.com");

			Assert.IsNull (part.ContentBase, "Initial ContentBase should be null");

			part.ContentBase = uri;
			Assert.AreEqual (uri, part.ContentBase, "Expected ContentBase to be updated");
			Assert.IsTrue (part.Headers.Contains (HeaderId.ContentBase), "Expected header to exist");

			part.ContentBase = null;
			Assert.IsNull (part.ContentBase, "Expected ContentBase to be null again");
			Assert.IsFalse (part.Headers.Contains (HeaderId.ContentBase), "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentBase, uri.OriginalString);
			Assert.AreEqual (uri, part.ContentBase, "Expected ContentBase to be set again");

			part.Headers.Remove (HeaderId.ContentBase);
			Assert.IsNull (part.ContentBase, "Expected ContentBase to be null again");
		}

		[Test]
		public void TestContentLocation ()
		{
			var part = new MimePart ();
			var uri = new Uri ("http://www.google.com");

			Assert.IsNull (part.ContentLocation, "Initial ContentLocation should be null");

			part.ContentLocation = uri;
			Assert.AreEqual (uri, part.ContentLocation, "Expected ContentLocation to be updated");
			Assert.IsTrue (part.Headers.Contains (HeaderId.ContentLocation), "Expected header to exist");

			part.ContentLocation = null;
			Assert.IsNull (part.ContentLocation, "Expected ContentLocation to be null again");
			Assert.IsFalse (part.Headers.Contains (HeaderId.ContentLocation), "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentLocation, uri.OriginalString);
			Assert.AreEqual (uri, part.ContentLocation, "Expected ContentLocation to be set again");

			part.Headers.Remove (HeaderId.ContentLocation);
			Assert.IsNull (part.ContentLocation, "Expected ContentLocation to be null again");
		}

		[Test]
		public void TestContentDuration ()
		{
			var part = new MimePart ();

			Assert.IsNull (part.ContentDuration, "Initial ContentDuration value should be null");

			part.ContentDuration = 500;
			Assert.AreEqual (500, part.ContentDuration, "Expected ContentDuration to be updated");
			Assert.IsTrue (part.Headers.Contains (HeaderId.ContentDuration), "Expected header to exist");

			part.ContentDuration = null;
			Assert.IsNull (part.ContentDuration, "Expected ContentDuration to be null again");
			Assert.IsFalse (part.Headers.Contains (HeaderId.ContentDuration), "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentDuration, "500");
			Assert.AreEqual (500, part.ContentDuration, "Expected ContentDuration to be set again");

			part.Headers.Remove (HeaderId.ContentDuration);
			Assert.IsNull (part.ContentDuration, "Expected ContentDuration to be null again");
		}

		[Test]
		public void TestContentId ()
		{
			var id = MimeUtils.GenerateMessageId ();
			var part = new MimePart ();

			Assert.IsNull (part.ContentId, "Initial ContentId value should be null");

			part.ContentId = id;
			Assert.AreEqual (id, part.ContentId, "Expected ContentId to be updated");
			Assert.IsTrue (part.Headers.Contains (HeaderId.ContentId), "Expected header to exist");

			part.ContentId = null;
			Assert.IsNull (part.ContentId, "Expected ContentId to be null again");
			Assert.IsFalse (part.Headers.Contains (HeaderId.ContentId), "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentId, string.Format ("<{0}>", id));
			Assert.AreEqual (id, part.ContentId, "Expected ContentId to be set again");

			part.Headers.Remove (HeaderId.ContentId);
			Assert.IsNull (part.ContentId, "Expected ContentId to be null again");
		}

		[Test]
		public void TestContentMd5 ()
		{
			var part = new MimePart ();

			Assert.IsNull (part.ContentMd5, "Initial ContentMd5 value should be null");

			part.ContentMd5 = "XYZ";
			Assert.AreEqual ("XYZ", part.ContentMd5, "Expected ContentMd5 to be updated");
			Assert.IsTrue (part.Headers.Contains (HeaderId.ContentMd5), "Expected header to exist");

			part.ContentMd5 = null;
			Assert.IsNull (part.ContentMd5, "Expected ContentMd5 to be null again");
			Assert.IsFalse (part.Headers.Contains (HeaderId.ContentMd5), "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentMd5, "XYZ");
			Assert.AreEqual ("XYZ", part.ContentMd5, "Expected ContentMd5 to be set again");

			part.Headers.Remove (HeaderId.ContentMd5);
			Assert.IsNull (part.ContentMd5, "Expected ContentMd5 to be null again");

			Assert.Throws<InvalidOperationException> (() => part.ComputeContentMd5 ());
			Assert.IsFalse (part.VerifyContentMd5 ());
		}

		[Test]
		public void TestContentTransferEncoding ()
		{
			var part = new MimePart ();

			Assert.AreEqual (ContentEncoding.Default, part.ContentTransferEncoding);

			part.ContentTransferEncoding = ContentEncoding.EightBit;
			Assert.AreEqual (ContentEncoding.EightBit, part.ContentTransferEncoding);
			Assert.IsTrue (part.Headers.Contains (HeaderId.ContentTransferEncoding), "Expected header to exist");

			Assert.Throws<ArgumentOutOfRangeException> (() => part.ContentTransferEncoding = (ContentEncoding) 500);

			part.ContentTransferEncoding = ContentEncoding.Default;
			Assert.AreEqual (ContentEncoding.Default, part.ContentTransferEncoding);
			Assert.IsFalse (part.Headers.Contains (HeaderId.ContentTransferEncoding), "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentTransferEncoding, "base64");
			Assert.AreEqual (ContentEncoding.Base64, part.ContentTransferEncoding, "Expected ContentTransferEncoding to be set again");

			part.Headers.Remove (HeaderId.ContentTransferEncoding);
			Assert.AreEqual (ContentEncoding.Default, part.ContentTransferEncoding, "Expected ContentTransferEncoding to be default again");
		}
	}
}
