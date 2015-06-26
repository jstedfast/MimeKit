//
// MultipartRelatedTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (www.xamarin.com)
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

namespace UnitTests {
	[TestFixture]
	public class MultipartRelatedTests
	{
		[Test]
		public void TestReferenceByContentId ()
		{
			var builder = new BodyBuilder ();

			builder.HtmlBody = "<html>This is an <b>html</b> body.</html>";

			builder.LinkedResources.Add ("empty.gif", new byte[0], new ContentType ("image", "gif"));
			builder.LinkedResources.Add ("empty.jpg", new byte[0], new ContentType ("image", "jpg"));

			foreach (var attachment in builder.LinkedResources)
				attachment.ContentId = MimeUtils.GenerateMessageId ();

			var body = builder.ToMessageBody ();

			Assert.IsInstanceOf<MultipartRelated> (body, "Expected a multipart/related.");

			var related = (MultipartRelated) body;

			Assert.AreEqual ("text/html", related.ContentType.Parameters["type"], "The type parameter does not match.");

			var root = related.Root;

			Assert.IsNotNull (root, "The root document should not be null.");
			Assert.IsTrue (root.ContentType.Matches ("text", "html"), "The root document has an unexpected mime-type.");

			// Note: MimeKit no longer sets the "start" parameter if the root is the first MIME part due to a bug in Thunderbird.
			//var start = "<" + root.ContentId + ">";

			//Assert.AreEqual (start, related.ContentType.Parameters["start"], "The start parameter does not match.");

			for (int i = 0; i < related.Count; i++) {
				var cid = new Uri (string.Format ("cid:{0}", related[i].ContentId));

				Assert.AreEqual (i, related.IndexOf (cid), "IndexOf did not return the expected index.");
			}
		}

		[Test]
		public void TestReferenceByContentLocation ()
		{
			var builder = new BodyBuilder ();

			builder.HtmlBody = "<html>This is an <b>html</b> body.</html>";

			builder.LinkedResources.Add ("empty.gif", new byte[0], new ContentType ("image", "gif"));
			builder.LinkedResources.Add ("empty.jpg", new byte[0], new ContentType ("image", "jpg"));

			var body = builder.ToMessageBody ();

			Assert.IsInstanceOf<MultipartRelated> (body, "Expected a multipart/related.");

			var related = (MultipartRelated) body;

			Assert.AreEqual ("text/html", related.ContentType.Parameters["type"], "The type parameter does not match.");

			var root = related.Root;

			Assert.IsNotNull (root, "The root document should not be null.");
			Assert.IsTrue (root.ContentType.Matches ("text", "html"), "The root document has an unexpected mime-type.");

			// Note: MimeKit no longer sets the "start" parameter if the root is the first MIME part due to a bug in Thunderbird.
			//var start = "<" + root.ContentId + ">";

			//Assert.AreEqual (start, related.ContentType.Parameters["start"], "The start parameter does not match.");

			for (int i = 1; i < related.Count; i++)
				Assert.AreEqual (i, related.IndexOf (related[i].ContentLocation), "IndexOf did not return the expected index.");
		}
	}
}
