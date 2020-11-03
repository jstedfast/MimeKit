//
// MultipartRelatedTests.cs
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

using NUnit.Framework;

using MimeKit;
using MimeKit.Utils;

namespace UnitTests {
	[TestFixture]
	public class MultipartRelatedTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var related = new MultipartRelated ();
			string mimeType, charset;

			Assert.Throws<ArgumentNullException> (() => new MultipartRelated ((MimeEntityConstructorArgs) null));
			Assert.Throws<ArgumentNullException> (() => related.Open (null, out mimeType, out charset));
			Assert.Throws<ArgumentNullException> (() => related.Open (null));
			Assert.Throws<ArgumentNullException> (() => related.Contains ((Uri) null));
			Assert.Throws<ArgumentNullException> (() => related.IndexOf ((Uri) null));
			Assert.Throws<ArgumentNullException> (() => related.Accept (null));
			Assert.Throws<ArgumentNullException> (() => related.Root = null);

			Assert.Throws<FileNotFoundException> (() => related.Open (new Uri ("http://www.xamarin.com/logo.png"), out mimeType, out charset));
			Assert.Throws<FileNotFoundException> (() => related.Open (new Uri ("http://www.xamarin.com/logo.png")));
		}

		[Test]
		public void TestDocumentRoot ()
		{
			var gif = new MimePart ("image", "gif") { ContentDisposition = new ContentDisposition (ContentDisposition.Inline) { FileName = "empty.gif" }, ContentId = MimeUtils.GenerateMessageId () };
			var jpg = new MimePart ("image", "jpg") { ContentDisposition = new ContentDisposition (ContentDisposition.Inline) { FileName = "empty.jpg" }, ContentId = MimeUtils.GenerateMessageId () };
			var html = new TextPart ("html") { Text = "This is the html body...", ContentId = MimeUtils.GenerateMessageId () };
			var related = new MultipartRelated (gif, jpg, html);
			string start;

			related.ContentType.Parameters["type"] = "text/html";
			related.ContentType.Parameters["start"] = "<" + html.ContentId + ">";

			Assert.AreEqual (3, related.Count, "Initial Count");
			Assert.AreEqual (html, related.Root, "Initial Root");
			Assert.AreEqual (html, related[2], "Initial Root should be the 3rd item.");

			var root = new TextPart ("html") { Text = "This is the replacement root document..." };

			related.Root = root;

			Assert.AreEqual (3, related.Count, "Count");
			Assert.AreEqual (root, related.Root, "Root");
			Assert.AreEqual (root, related[2], "Root should be the 3rd item.");
			Assert.IsNotNull (root.ContentId, "Root's Content-Id should not be null.");
			Assert.IsNotEmpty (root.ContentId, "Root's Content-Id should not be empty.");

			start = "<" + root.ContentId + ">";

			Assert.AreEqual (start, related.ContentType.Parameters["start"], "The start parameter does not match.");

			related.Clear ();
			related.Add (gif);
			related.Add (jpg);
			related.Root = html;

			Assert.AreEqual (3, related.Count, "Count");
			Assert.AreEqual (html, related.Root, "Root");
			Assert.AreEqual (html, related[0], "Root should be the 1st item.");

			// Note: MimeKit no longer sets the "start" parameter if the root is the first MIME part due to a bug in Thunderbird.
			Assert.IsNull (related.ContentType.Parameters["start"], "The start parameter should be null.");
		}

		[Test]
		public void TestDocumentRootByType ()
		{
			var related = (MultipartRelated) MimeEntity.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "multipart-related-mhtml.txt"));

			Assert.AreEqual (2, related.Count, "Count");

			var image = related[0];

			Assert.AreEqual ("image/png", image.ContentType.MimeType, "related[0]");

			var html = related[1];

			Assert.AreEqual ("text/html", html.ContentType.MimeType, "related[1]");

			Assert.AreEqual (html, related.Root, "Root");
		}

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
			Assert.IsTrue (root.ContentType.IsMimeType ("text", "html"), "The root document has an unexpected mime-type.");

			// Note: MimeKit no longer sets the "start" parameter if the root is the first MIME part due to a bug in Thunderbird.
			Assert.IsNull (related.ContentType.Parameters["start"], "The start parameter should be null.");

			for (int i = 1; i < related.Count; i++) {
				var cid = new Uri (string.Format ("cid:{0}", related[i].ContentId));
				string mimeType, charset;

				Assert.IsTrue (related.Contains (cid), "Contains failed.");
				Assert.AreEqual (i, related.IndexOf (cid), "IndexOf did not return the expected index.");

				using (var stream = related.Open (cid, out mimeType, out charset)) {
					Assert.AreEqual (related[i].ContentType.MimeType, mimeType, "mime-types did not match.");
				}

				Assert.DoesNotThrow (() => related.Open (cid).Dispose ());
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
			Assert.IsTrue (root.ContentType.IsMimeType ("text", "html"), "The root document has an unexpected mime-type.");

			// Note: MimeKit no longer sets the "start" parameter if the root is the first MIME part due to a bug in Thunderbird.
			Assert.IsNull (related.ContentType.Parameters["start"], "The start parameter should be null.");

			for (int i = 1; i < related.Count; i++) {
				string mimeType, charset;

				Assert.IsTrue (related.Contains (related[i].ContentLocation), "Contains failed.");
				Assert.AreEqual (i, related.IndexOf (related[i].ContentLocation), "IndexOf did not return the expected index.");

				using (var stream = related.Open (related[i].ContentLocation, out mimeType, out charset)) {
					Assert.AreEqual (related[i].ContentType.MimeType, mimeType, "mime-types did not match.");
				}

				Assert.DoesNotThrow (() => related.Open (related[i].ContentLocation).Dispose ());
			}
		}
	}
}
