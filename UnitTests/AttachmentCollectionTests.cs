//
// AttachmentCollectionTests.cs
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

namespace UnitTests
{
	[TestFixture]
	public class AttachmentCollectionTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var contentType = new ContentType ("application", "octet-stream");
			var attachments = new AttachmentCollection ();
			var items = new MimeEntity[10];
			var data = new byte[1024];

			using (var stream = new MemoryStream ()) {
				Assert.Throws<ArgumentException> (() => attachments.Add (string.Empty));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((string) null));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((MimeEntity) null));
				Assert.Throws<ArgumentException> (() => attachments.Add (string.Empty, data));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((string) null, data));
				Assert.Throws<ArgumentException> (() => attachments.Add (string.Empty, stream));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((string) null, stream));
				Assert.Throws<ArgumentException> (() => attachments.Add (string.Empty, contentType));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((string) null, contentType));
				Assert.Throws<ArgumentException> (() => attachments.Add (string.Empty, data, contentType));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((string) null, data, contentType));
				Assert.Throws<ArgumentException> (() => attachments.Add (string.Empty, stream, contentType));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((string) null, stream, contentType));

				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", (byte[]) null));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", (Stream) null));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", (byte[]) null, contentType));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", (Stream) null, contentType));

				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", (ContentType) null));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", data, null));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", stream, null));

				Assert.Throws<ArgumentNullException> (() => attachments.Contains (null));

				Assert.Throws<ArgumentNullException> (() => attachments.CopyTo (null, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => attachments.CopyTo (items, -1));

				Assert.Throws<ArgumentNullException> (() => attachments.IndexOf (null));

				Assert.Throws<ArgumentNullException> (() => attachments.Remove (null));
				Assert.Throws<ArgumentOutOfRangeException> (() => attachments.RemoveAt (0));

				attachments.Add (new TextPart ("plain"));
				Assert.Throws<ArgumentOutOfRangeException> (() => { var x = attachments[10]; });
				Assert.Throws<ArgumentOutOfRangeException> (() => attachments[10] = new TextPart ("plain"));
				Assert.Throws<ArgumentNullException> (() => attachments[0] = null);

				Assert.Throws<ArgumentOutOfRangeException> (() => attachments.Insert (-1, new TextPart ("plain")));
				Assert.Throws<ArgumentNullException> (() => attachments.Insert (0, null));
			}
		}

		[Test]
		public void TestAddFileName ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName);
			Assert.AreEqual ("image/jpeg", attachment.ContentType.MimeType);
			Assert.AreEqual ("girl.jpg", attachment.ContentType.Name);
			Assert.NotNull (attachment.ContentDisposition);
			Assert.AreEqual ("attachment", attachment.ContentDisposition.Disposition);
			Assert.AreEqual ("girl.jpg", attachment.ContentDisposition.FileName);
			Assert.AreEqual ("girl.jpg", attachment.FileName);
			Assert.AreEqual (ContentEncoding.Base64, attachment.ContentTransferEncoding);
			Assert.AreEqual (1, attachments.Count);

			Assert.IsTrue (attachments.Contains (attachment), "Contains");
			Assert.AreEqual (0, attachments.IndexOf (attachment), "IndexOf");
			Assert.IsTrue (attachments.Remove (attachment), "Remove");
			Assert.AreEqual (0, attachments.Count);
			attachments.Clear ();
		}

		[Test]
		public void TestAddInlineFileName ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection (true);
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName);
			Assert.AreEqual ("image/jpeg", attachment.ContentType.MimeType);
			Assert.AreEqual ("girl.jpg", attachment.ContentType.Name);
			Assert.NotNull (attachment.ContentDisposition);
			Assert.AreEqual ("inline", attachment.ContentDisposition.Disposition);
			Assert.AreEqual ("girl.jpg", attachment.ContentDisposition.FileName);
			Assert.AreEqual ("girl.jpg", attachment.FileName);
			Assert.AreEqual (ContentEncoding.Base64, attachment.ContentTransferEncoding);
			Assert.AreEqual (1, attachments.Count);

			Assert.IsTrue (attachments.Contains (attachment), "Contains");
			Assert.AreEqual (0, attachments.IndexOf (attachment), "IndexOf");
			Assert.IsTrue (attachments.Remove (attachment), "Remove");
			Assert.AreEqual (0, attachments.Count);
			attachments.Clear ();
		}

		[Test]
		public void TestAddFileNameContentType ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var contentType = new ContentType ("image", "gif");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName, contentType);
			Assert.AreEqual (contentType.MimeType, attachment.ContentType.MimeType);
			Assert.AreEqual ("girl.jpg", attachment.ContentType.Name);
			Assert.NotNull (attachment.ContentDisposition);
			Assert.AreEqual ("attachment", attachment.ContentDisposition.Disposition);
			Assert.AreEqual ("girl.jpg", attachment.ContentDisposition.FileName);
			Assert.AreEqual ("girl.jpg", attachment.FileName);
			Assert.AreEqual (ContentEncoding.Base64, attachment.ContentTransferEncoding);
			Assert.AreEqual (1, attachments.Count);

			Assert.IsTrue (attachments.Contains (attachment), "Contains");
			Assert.AreEqual (0, attachments.IndexOf (attachment), "IndexOf");
			Assert.IsTrue (attachments.Remove (attachment), "Remove");
			Assert.AreEqual (0, attachments.Count);
			attachments.Clear ();
		}

		[Test]
		public void TestAddData ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName, File.ReadAllBytes (fileName));
			Assert.AreEqual ("image/jpeg", attachment.ContentType.MimeType);
			Assert.AreEqual ("girl.jpg", attachment.ContentType.Name);
			Assert.NotNull (attachment.ContentDisposition);
			Assert.AreEqual ("attachment", attachment.ContentDisposition.Disposition);
			Assert.AreEqual ("girl.jpg", attachment.ContentDisposition.FileName);
			Assert.AreEqual ("girl.jpg", attachment.FileName);
			Assert.AreEqual (ContentEncoding.Base64, attachment.ContentTransferEncoding);
			Assert.AreEqual (1, attachments.Count);

			Assert.IsTrue (attachments.Contains (attachment), "Contains");
			Assert.AreEqual (0, attachments.IndexOf (attachment), "IndexOf");
			Assert.IsTrue (attachments.Remove (attachment), "Remove");
			Assert.AreEqual (0, attachments.Count);
			attachments.Clear ();
		}

		[Test]
		public void TestAddDataContentType ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var contentType = new ContentType ("image", "gif");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName, File.ReadAllBytes (fileName), contentType);
			Assert.AreEqual (contentType.MimeType, attachment.ContentType.MimeType);
			Assert.AreEqual ("girl.jpg", attachment.ContentType.Name);
			Assert.NotNull (attachment.ContentDisposition);
			Assert.AreEqual ("attachment", attachment.ContentDisposition.Disposition);
			Assert.AreEqual ("girl.jpg", attachment.ContentDisposition.FileName);
			Assert.AreEqual ("girl.jpg", attachment.FileName);
			Assert.AreEqual (ContentEncoding.Base64, attachment.ContentTransferEncoding);
			Assert.AreEqual (1, attachments.Count);

			Assert.IsTrue (attachments.Contains (attachment), "Contains");
			Assert.AreEqual (0, attachments.IndexOf (attachment), "IndexOf");
			Assert.IsTrue (attachments.Remove (attachment), "Remove");
			Assert.AreEqual (0, attachments.Count);
			attachments.Clear ();
		}

		[Test]
		public void TestAddStream ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = (MimePart) attachments.Add (fileName, stream);

			Assert.AreEqual ("image/jpeg", attachment.ContentType.MimeType);
			Assert.AreEqual ("girl.jpg", attachment.ContentType.Name);
			Assert.NotNull (attachment.ContentDisposition);
			Assert.AreEqual ("attachment", attachment.ContentDisposition.Disposition);
			Assert.AreEqual ("girl.jpg", attachment.ContentDisposition.FileName);
			Assert.AreEqual ("girl.jpg", attachment.FileName);
			Assert.AreEqual (ContentEncoding.Base64, attachment.ContentTransferEncoding);
			Assert.AreEqual (1, attachments.Count);

			Assert.IsTrue (attachments.Contains (attachment), "Contains");
			Assert.AreEqual (0, attachments.IndexOf (attachment), "IndexOf");
			Assert.IsTrue (attachments.Remove (attachment), "Remove");
			Assert.AreEqual (0, attachments.Count);
			attachments.Clear ();
		}

		[Test]
		public void TestAddStreamContentType ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var contentType = new ContentType ("image", "gif");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = (MimePart) attachments.Add (fileName, stream, contentType);

			Assert.AreEqual (contentType.MimeType, attachment.ContentType.MimeType);
			Assert.AreEqual ("girl.jpg", attachment.ContentType.Name);
			Assert.NotNull (attachment.ContentDisposition);
			Assert.AreEqual ("attachment", attachment.ContentDisposition.Disposition);
			Assert.AreEqual ("girl.jpg", attachment.ContentDisposition.FileName);
			Assert.AreEqual ("girl.jpg", attachment.FileName);
			Assert.AreEqual (ContentEncoding.Base64, attachment.ContentTransferEncoding);
			Assert.AreEqual (1, attachments.Count);

			Assert.IsTrue (attachments.Contains (attachment), "Contains");
			Assert.AreEqual (0, attachments.IndexOf (attachment), "IndexOf");
			Assert.IsTrue (attachments.Remove (attachment), "Remove");
			Assert.AreEqual (0, attachments.Count);
			attachments.Clear ();
		}

		[Test]
		public void TestAddTextFileName ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "lorem-ipsum.txt");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName);
			Assert.AreEqual ("text/plain", attachment.ContentType.MimeType);
			Assert.AreEqual ("lorem-ipsum.txt", attachment.ContentType.Name);
			Assert.NotNull (attachment.ContentDisposition);
			Assert.AreEqual ("attachment", attachment.ContentDisposition.Disposition);
			Assert.AreEqual ("lorem-ipsum.txt", attachment.ContentDisposition.FileName);
			Assert.AreEqual ("lorem-ipsum.txt", attachment.FileName);
			Assert.AreEqual (ContentEncoding.SevenBit, attachment.ContentTransferEncoding);
			Assert.AreEqual (1, attachments.Count);

			Assert.IsTrue (attachments.Contains (attachment), "Contains");
			Assert.AreEqual (0, attachments.IndexOf (attachment), "IndexOf");
			Assert.IsTrue (attachments.Remove (attachment), "Remove");
			Assert.AreEqual (0, attachments.Count);
			attachments.Clear ();
		}

		[Test]
		public void TestAddEmailMessage ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.1.txt");
			var attachments = new AttachmentCollection ();
			MimeEntity attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = attachments.Add ("message.eml", stream);

			Assert.AreEqual ("message/rfc822", attachment.ContentType.MimeType);
			Assert.AreEqual ("message.eml", attachment.ContentType.Name);
			Assert.NotNull (attachment.ContentDisposition);
			Assert.AreEqual ("attachment", attachment.ContentDisposition.Disposition);
			Assert.AreEqual ("message.eml", attachment.ContentDisposition.FileName);
			Assert.AreEqual (1, attachments.Count);

			Assert.IsTrue (attachments.Contains (attachment), "Contains");
			Assert.AreEqual (0, attachments.IndexOf (attachment), "IndexOf");
			Assert.IsTrue (attachments.Remove (attachment), "Remove");
			Assert.AreEqual (0, attachments.Count);
			attachments.Clear ();
		}

		[Test]
		public void TestAddInlineEmailMessage ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.1.txt");
			var attachments = new AttachmentCollection (true);
			MimeEntity attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = attachments.Add ("message.eml", stream);

			Assert.AreEqual ("message/rfc822", attachment.ContentType.MimeType);
			Assert.AreEqual ("message.eml", attachment.ContentType.Name);
			Assert.NotNull (attachment.ContentDisposition);
			Assert.AreEqual ("inline", attachment.ContentDisposition.Disposition);
			Assert.AreEqual ("message.eml", attachment.ContentDisposition.FileName);
			Assert.AreEqual (1, attachments.Count);

			Assert.IsTrue (attachments.Contains (attachment), "Contains");
			Assert.AreEqual (0, attachments.IndexOf (attachment), "IndexOf");
			Assert.IsTrue (attachments.Remove (attachment), "Remove");
			Assert.AreEqual (0, attachments.Count);
			attachments.Clear ();
		}
	}
}
