//
// AttachmentCollectionTests.cs
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

using System.Collections;

using MimeKit;

namespace UnitTests {
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

			Assert.That (attachments.IsReadOnly, Is.False, "IsReadOnly");

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

				Assert.ThrowsAsync<ArgumentException> (async () => await attachments.AddAsync (string.Empty));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await attachments.AddAsync ((string) null));
				Assert.ThrowsAsync<ArgumentException> (async () => await attachments.AddAsync (string.Empty, stream));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await attachments.AddAsync ((string) null, stream));
				Assert.ThrowsAsync<ArgumentException> (async () => await attachments.AddAsync (string.Empty, contentType));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await attachments.AddAsync ((string) null, contentType));
				Assert.ThrowsAsync<ArgumentException> (async () => await attachments.AddAsync (string.Empty, stream, contentType));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await attachments.AddAsync ((string) null, stream, contentType));

				Assert.ThrowsAsync<ArgumentNullException> (async () => await attachments.AddAsync ("file.dat", (Stream) null));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await attachments.AddAsync ("file.dat", (Stream) null, contentType));

				Assert.ThrowsAsync<ArgumentNullException> (async () => await attachments.AddAsync ("file.dat", (ContentType) null));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await attachments.AddAsync ("file.dat", stream, null));

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
		public void TestClear ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName);
			attachments.Clear ();

			Assert.That (attachments.Count, Is.EqualTo (0));
			Assert.That (attachment.IsDisposed, Is.False, "Attachment should not have been disposed after Clear().");
			attachment.Dispose ();

			attachment = (MimePart) attachments.Add (fileName);
			attachments.Clear (true);

			Assert.That (attachments.Count, Is.EqualTo (0));
			Assert.That (attachment.IsDisposed, Is.True, "Attachment should have been disposed after Clear(true).");
		}

		[Test]
		public void TestAddFileName ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName);
			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("image/jpeg"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public async Task TestAddFileNameAsync ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) await attachments.AddAsync (fileName);
			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("image/jpeg"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public void TestAddInlineFileName ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection (true);
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName);
			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("image/jpeg"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("inline"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public async Task TestAddInlineFileNameAsync ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection (true);
			MimePart attachment;

			attachment = (MimePart) await attachments.AddAsync (fileName);
			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("image/jpeg"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("inline"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public void TestAddFileNameContentType ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var contentType = new ContentType ("image", "gif");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName, contentType);
			Assert.That (attachment.ContentType.MimeType, Is.EqualTo (contentType.MimeType));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public async Task TestAddFileNameContentTypeAsync ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var contentType = new ContentType ("image", "gif");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) await attachments.AddAsync (fileName, contentType);
			Assert.That (attachment.ContentType.MimeType, Is.EqualTo (contentType.MimeType));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public void TestAddData ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName, File.ReadAllBytes (fileName));
			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("image/jpeg"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public void TestAddDataContentType ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var contentType = new ContentType ("image", "gif");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName, File.ReadAllBytes (fileName), contentType);
			Assert.That (attachment.ContentType.MimeType, Is.EqualTo (contentType.MimeType));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public void TestAddStream ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = (MimePart) attachments.Add (fileName, stream);

			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("image/jpeg"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public async Task TestAddStreamAsync ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = (MimePart) await attachments.AddAsync (fileName, stream);

			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("image/jpeg"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
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

			Assert.That (attachment.ContentType.MimeType, Is.EqualTo (contentType.MimeType));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public async Task TestAddStreamContentTypeAsync ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var contentType = new ContentType ("image", "gif");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = (MimePart) await attachments.AddAsync (fileName, stream, contentType);

			Assert.That (attachment.ContentType.MimeType, Is.EqualTo (contentType.MimeType));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.FileName, Is.EqualTo ("girl.jpg"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public void TestAddTextFileName ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "lorem-ipsum.txt");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) attachments.Add (fileName);
			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("text/plain"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("lorem-ipsum.txt"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("lorem-ipsum.txt"));
			Assert.That (attachment.FileName, Is.EqualTo ("lorem-ipsum.txt"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public async Task TestAddTextFileNameAsync ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "lorem-ipsum.txt");
			var attachments = new AttachmentCollection ();
			MimePart attachment;

			attachment = (MimePart) await attachments.AddAsync (fileName);
			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("text/plain"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("lorem-ipsum.txt"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("lorem-ipsum.txt"));
			Assert.That (attachment.FileName, Is.EqualTo ("lorem-ipsum.txt"));
			Assert.That (attachment.ContentTransferEncoding, Is.EqualTo (ContentEncoding.SevenBit));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public void TestAddEmailMessage ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.1.txt");
			var attachments = new AttachmentCollection ();
			MimeEntity attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = attachments.Add ("message.eml", stream);

			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("message/rfc822"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("message.eml"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("message.eml"));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public async Task TestAddEmailMessageAsync ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.1.txt");
			var attachments = new AttachmentCollection ();
			MimeEntity attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = await attachments.AddAsync ("message.eml", stream);

			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("message/rfc822"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("message.eml"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("message.eml"));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public void TestAddEmailMessageFallback ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection ();
			MimeEntity attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = attachments.Add ("message.eml", stream);

			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("application/octet-stream"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("message.eml"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("message.eml"));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public async Task TestAddEmailMessageFallbackAsync ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var attachments = new AttachmentCollection ();
			MimeEntity attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = await attachments.AddAsync ("message.eml", stream);

			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("application/octet-stream"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("message.eml"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("attachment"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("message.eml"));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public void TestAddInlineEmailMessage ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.1.txt");
			var attachments = new AttachmentCollection (true);
			MimeEntity attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = attachments.Add ("message.eml", stream);

			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("message/rfc822"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("message.eml"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("inline"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("message.eml"));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public async Task TestAddInlineEmailMessageAsync ()
		{
			var fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "messages", "body.1.txt");
			var attachments = new AttachmentCollection (true);
			MimeEntity attachment;

			using (var stream = File.OpenRead (fileName))
				attachment = await attachments.AddAsync ("message.eml", stream);

			Assert.That (attachment.ContentType.MimeType, Is.EqualTo ("message/rfc822"));
			Assert.That (attachment.ContentType.Name, Is.EqualTo ("message.eml"));
			Assert.That (attachment.ContentDisposition, Is.Not.Null);
			Assert.That (attachment.ContentDisposition.Disposition, Is.EqualTo ("inline"));
			Assert.That (attachment.ContentDisposition.FileName, Is.EqualTo ("message.eml"));
			Assert.That (attachments.Count, Is.EqualTo (1));

			Assert.That (attachments.Contains (attachment), Is.True, "Contains");
			Assert.That (attachments.IndexOf (attachment), Is.EqualTo (0), "IndexOf");
			Assert.That (attachments.Remove (attachment), Is.True, "Remove");
			Assert.That (attachments.Count, Is.EqualTo (0));
			attachments.Clear (true);
		}

		[Test]
		public void TestListMethods ()
		{
			var attachments = new AttachmentCollection ();
			string fileName;

			fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "lorem-ipsum.txt");
			var plain = (MimePart) attachments.Add (fileName);

			fileName = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var jpeg = (MimePart) attachments.Add (fileName);

			var copied = new MimeEntity[2];
			attachments.CopyTo (copied, 0);

			Assert.That (copied[0], Is.EqualTo (plain));
			Assert.That (copied[1], Is.EqualTo (jpeg));

			attachments.RemoveAt (0);
			Assert.That (attachments.Count, Is.EqualTo (1));
			Assert.That (attachments[0], Is.EqualTo (jpeg));

			attachments[0] = plain;
			Assert.That (attachments.Count, Is.EqualTo (1));
			Assert.That (attachments[0], Is.EqualTo (plain));

			attachments.Insert (0, jpeg);
			Assert.That (attachments.Count, Is.EqualTo (2));
			Assert.That (attachments[0], Is.EqualTo (jpeg));
			Assert.That (attachments[1], Is.EqualTo (plain));

			int i = 0;
			foreach (MimeEntity attachment in (IEnumerable) attachments)
				copied[i++] = attachment;

			Assert.That (copied[0], Is.EqualTo (jpeg));
			Assert.That (copied[1], Is.EqualTo (plain));
		}
	}
}
