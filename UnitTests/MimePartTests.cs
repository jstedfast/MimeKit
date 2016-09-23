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
