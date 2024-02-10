//
// MultipartRelatedTests.cs
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

using MimeKit;
using MimeKit.Text;
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
		public void TestGenericArgsConstructor ()
		{
			var multipart = new MultipartRelated (
				new Header (HeaderId.ContentDescription, "This is a description of the multipart."),
				new TextPart (TextFormat.Plain) { Text = "This is the message body." },
				new MimePart ("image", "gif") { FileName = "attachment.gif" }
				);

			Assert.That (multipart.Headers.Contains (HeaderId.ContentDescription), Is.True, "Content-Description header");
			Assert.That (multipart.Count, Is.EqualTo (2), "Child part count");
			Assert.That (multipart[0].ContentType.MimeType, Is.EqualTo ("text/plain"), "MimeType[0]");
			Assert.That (multipart[1].ContentType.MimeType, Is.EqualTo ("image/gif"), "MimeType[1]");
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

			Assert.That (related.Count, Is.EqualTo (3), "Initial Count");
			Assert.That (related.Root, Is.EqualTo (html), "Initial Root");
			Assert.That (related[2], Is.EqualTo (html), "Initial Root should be the 3rd item.");

			var root = new TextPart ("html") { Text = "This is the replacement root document..." };

			related.Root = root;

			Assert.That (related.Count, Is.EqualTo (3), "Count");
			Assert.That (related.Root, Is.EqualTo (root), "Root");
			Assert.That (related[2], Is.EqualTo (root), "Root should be the 3rd item.");
			Assert.That (root.ContentId, Is.Not.Null, "Root's Content-Id should not be null.");
			Assert.That (root.ContentId, Is.Not.Empty, "Root's Content-Id should not be empty.");

			start = "<" + root.ContentId + ">";

			Assert.That (related.ContentType.Parameters["start"], Is.EqualTo (start), "The start parameter does not match.");

			related.Clear ();
			related.Add (gif);
			related.Add (jpg);
			related.Root = html;

			Assert.That (related.Count, Is.EqualTo (3), "Count");
			Assert.That (related.Root, Is.EqualTo (html), "Root");
			Assert.That (related[0], Is.EqualTo (html), "Root should be the 1st item.");

			// Note: MimeKit no longer sets the "start" parameter if the root is the first MIME part due to a bug in Thunderbird.
			Assert.That (related.ContentType.Parameters["start"], Is.Null, "The start parameter should be null.");
		}

		[Test]
		public void TestDocumentRootByType ()
		{
			var related = (MultipartRelated) MimeEntity.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "multipart-related-mhtml.txt"));

			Assert.That (related.Count, Is.EqualTo (2), "Count");

			var image = related[0];

			Assert.That (image.ContentType.MimeType, Is.EqualTo ("image/png"), "related[0]");

			var html = related[1];

			Assert.That (html.ContentType.MimeType, Is.EqualTo ("text/html"), "related[1]");

			Assert.That (related.Root, Is.EqualTo (html), "Root");
		}

		[Test]
		public void TestReferenceByContentId ()
		{
			var builder = new BodyBuilder {
				HtmlBody = "<html>This is an <b>html</b> body.</html>"
			};

			builder.LinkedResources.Add ("empty.gif", Array.Empty<byte> (), new ContentType ("image", "gif"));
			builder.LinkedResources.Add ("empty.jpg", Array.Empty<byte> (), new ContentType ("image", "jpg"));

			foreach (var attachment in builder.LinkedResources)
				attachment.ContentId = MimeUtils.GenerateMessageId ();

			var body = builder.ToMessageBody ();

			Assert.That (body, Is.InstanceOf<MultipartRelated> (), "Expected a multipart/related.");

			var related = (MultipartRelated) body;

			Assert.That (related.ContentType.Parameters["type"], Is.EqualTo ("text/html"), "The type parameter does not match.");

			var root = related.Root;

			Assert.That (root, Is.Not.Null, "The root document should not be null.");
			Assert.That (root.ContentType.IsMimeType ("text", "html"), Is.True, "The root document has an unexpected mime-type.");

			// Note: MimeKit no longer sets the "start" parameter if the root is the first MIME part due to a bug in Thunderbird.
			Assert.That (related.ContentType.Parameters["start"], Is.Null, "The start parameter should be null.");

			for (int i = 1; i < related.Count; i++) {
				var cid = new Uri (string.Format ("cid:{0}", related[i].ContentId));

				Assert.That (related.Contains (cid), Is.True, "Contains failed.");
				Assert.That (related.IndexOf (cid), Is.EqualTo (i), "IndexOf did not return the expected index.");

				using (var stream = related.Open (cid, out var mimeType, out var charset)) {
					Assert.That (mimeType, Is.EqualTo (related[i].ContentType.MimeType), "mime-types did not match.");
				}

				Assert.DoesNotThrow (() => related.Open (cid).Dispose ());
			}
		}

		[Test]
		public void TestReferenceByContentLocation ()
		{
			var builder = new BodyBuilder {
				HtmlBody = "<html>This is an <b>html</b> body.</html>"
			};

			builder.LinkedResources.Add ("empty.gif", Array.Empty<byte> (), new ContentType ("image", "gif"));
			builder.LinkedResources.Add ("empty.jpg", Array.Empty<byte> (), new ContentType ("image", "jpg"));

			var body = builder.ToMessageBody ();

			Assert.That (body, Is.InstanceOf<MultipartRelated> (), "Expected a multipart/related.");

			var related = (MultipartRelated) body;

			Assert.That (related.ContentType.Parameters["type"], Is.EqualTo ("text/html"), "The type parameter does not match.");

			var root = related.Root;

			Assert.That (root, Is.Not.Null, "The root document should not be null.");
			Assert.That (root.ContentType.IsMimeType ("text", "html"), Is.True, "The root document has an unexpected mime-type.");

			// Note: MimeKit no longer sets the "start" parameter if the root is the first MIME part due to a bug in Thunderbird.
			Assert.That (related.ContentType.Parameters["start"], Is.Null, "The start parameter should be null.");

			for (int i = 1; i < related.Count; i++) {
				Assert.That (related.Contains (related[i].ContentLocation), Is.True, "Contains failed.");
				Assert.That (related.IndexOf (related[i].ContentLocation), Is.EqualTo (i), "IndexOf did not return the expected index.");

				using (var stream = related.Open (related[i].ContentLocation, out var mimeType, out var charset)) {
					Assert.That (mimeType, Is.EqualTo (related[i].ContentType.MimeType), "mime-types did not match.");
				}

				Assert.DoesNotThrow (() => related.Open (related[i].ContentLocation).Dispose ());
			}
		}
	}
}
