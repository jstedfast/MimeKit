//
// MimePartTests.cs
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

using System.Text;

using MimeKit;
using MimeKit.Utils;

namespace UnitTests {
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

			Assert.Throws<ArgumentOutOfRangeException> (() => part.ContentDuration = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => part.Prepare (EncodingConstraint.SevenBit, 1));

			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load ((Stream) null));
			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load ((Stream) null, true));
			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load ((ParserOptions) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load (ParserOptions.Default, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load (null, Stream.Null, true));
			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load (ParserOptions.Default, (Stream) null, true));

			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load ((ContentType) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load (new ContentType ("application", "octet-stream"), null));
			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load (null, new ContentType ("application", "octet-stream"), Stream.Null));
			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load (ParserOptions.Default, null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load (ParserOptions.Default, new ContentType ("application", "octet-stream"), null));

			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load ((string) null));
			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load (null, "fileName"));
			Assert.Throws<ArgumentNullException> (() => MimeEntity.Load (ParserOptions.Default, (string) null));

			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync ((Stream) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync ((Stream) null, true));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync ((ParserOptions) null, Stream.Null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync (ParserOptions.Default, (Stream) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync (null, Stream.Null, true));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync (ParserOptions.Default, (Stream) null, true));

			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync ((ContentType) null, Stream.Null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync (new ContentType ("application", "octet-stream"), null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync (null, new ContentType ("application", "octet-stream"), Stream.Null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync (ParserOptions.Default, null, Stream.Null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync (ParserOptions.Default, new ContentType ("application", "octet-stream"), null));

			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync ((string) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync (null, "fileName"));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await MimeEntity.LoadAsync (ParserOptions.Default, (string) null));

			Assert.Throws<ArgumentNullException> (() => part.Accept (null));
			Assert.Throws<ArgumentNullException> (() => part.WriteTo ((string) null));
			Assert.Throws<ArgumentNullException> (() => part.WriteTo ((Stream) null));
			Assert.Throws<ArgumentNullException> (() => part.WriteTo ((string) null, false));
			Assert.Throws<ArgumentNullException> (() => part.WriteTo ((Stream) null, false));
			Assert.Throws<ArgumentNullException> (() => part.WriteTo (null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => part.WriteTo (FormatOptions.Default, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => part.WriteTo (null, "fileName"));
			Assert.Throws<ArgumentNullException> (() => part.WriteTo (FormatOptions.Default, (string) null));
			Assert.Throws<ArgumentNullException> (() => part.WriteTo (null, Stream.Null, false));
			Assert.Throws<ArgumentNullException> (() => part.WriteTo (FormatOptions.Default, (Stream) null, false));
			Assert.Throws<ArgumentNullException> (() => part.WriteTo (null, "fileName", false));
			Assert.Throws<ArgumentNullException> (() => part.WriteTo (FormatOptions.Default, (string) null, false));
			Assert.Throws<ArgumentException> (() => part.ContentId = "<image.jpg");

			Assert.ThrowsAsync<ArgumentNullException> (async () => await part.WriteToAsync ((string) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await part.WriteToAsync ((Stream) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await part.WriteToAsync ((string) null, false));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await part.WriteToAsync ((Stream) null, false));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await part.WriteToAsync (null, Stream.Null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await part.WriteToAsync (FormatOptions.Default, (Stream) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await part.WriteToAsync (null, "fileName"));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await part.WriteToAsync (FormatOptions.Default, (string) null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await part.WriteToAsync (null, Stream.Null, false));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await part.WriteToAsync (FormatOptions.Default, (Stream) null, false));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await part.WriteToAsync (null, "fileName", false));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await part.WriteToAsync (FormatOptions.Default, (string) null, false));
		}

		[Test]
		public void TestParameterizedCtor ()
		{
			const string expected = "Content-Type: text/plain\nContent-Transfer-Encoding: base64\nContent-Id: <id@localhost.com>\n\n";
			var headers = new [] { new Header ("Content-Id", "<id@localhost.com>") };
			var part = new MimePart ("text", "plain", new Header ("Content-Transfer-Encoding", "base64"), headers) {
				Content = new MimeContent (new MemoryStream ())
			};

			Assert.That (part.ContentId, Is.EqualTo ("id@localhost.com"), "Content-Id");
			Assert.That (part.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64), "Content-Transfer-Encoding");

			using (var stream = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Unix;

				part.WriteTo (options, stream);

				var serialized = Encoding.ASCII.GetString (stream.GetBuffer (), 0, (int) stream.Length);
				Assert.That (serialized, Is.EqualTo (expected), "Serialized");
			}
		}

		[Test]
		public void TestContentDisposition ()
		{
			var part = new MimePart ();

			Assert.That (part.ContentDisposition, Is.Null, "Initial ContentDisposition property should be null");

			part.ContentDisposition = new ContentDisposition (ContentDisposition.Attachment);
			Assert.That (part.ContentDisposition, Is.Not.Null, "Expected ContentDisposition property to be set");
			Assert.That (part.Headers.Contains (HeaderId.ContentDisposition), Is.True, "Expected header to exist");

			part.ContentDisposition = null;
			Assert.That (part.ContentDisposition, Is.Null, "Expected ContentDisposition property to be null again");
			Assert.That (part.Headers.Contains (HeaderId.ContentDisposition), Is.False, "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentDisposition, "attachment");
			Assert.That (part.ContentDisposition, Is.Not.Null, "Expected ContentDisposition property to be set again");

			part.Headers.Remove (HeaderId.ContentDisposition);
			Assert.That (part.ContentDisposition, Is.Null, "Expected ContentDisposition to be null again");

			part.ContentDisposition = new ContentDisposition ();
			part.FileName = "fileName";
			part.Headers.Clear ();
			Assert.That (part.ContentDisposition, Is.Null, "Expected ContentDisposition to be null again");
		}

		[Test]
		public void TestIsAttachment ()
		{
			var part = new MimePart ();

			Assert.That (part.ContentDisposition, Is.Null, "Initial ContentDisposition should be null");

			part.IsAttachment = true;

			Assert.That (part.ContentDisposition, Is.Not.Null, "Expected ContentDisposition property to be set");
			Assert.That (part.Headers.Contains (HeaderId.ContentDisposition), Is.True, "Expected header to exist");
			Assert.That (part.ContentDisposition.Disposition, Is.EqualTo (ContentDisposition.Attachment), "Expected Content-Disposition value to be attachment");

			part.IsAttachment = false;

			Assert.That (part.ContentDisposition, Is.Not.Null, "Expected ContentDisposition property to still be set");
			Assert.That (part.Headers.Contains (HeaderId.ContentDisposition), Is.True, "Expected header to still exist");
			Assert.That (part.ContentDisposition.Disposition, Is.EqualTo (ContentDisposition.Inline), "Expected Content-Disposition value to be inline");

			part.IsAttachment = true;

			Assert.That (part.ContentDisposition, Is.Not.Null, "Expected ContentDisposition property to still be set");
			Assert.That (part.Headers.Contains (HeaderId.ContentDisposition), Is.True, "Expected header to still exist");
			Assert.That (part.ContentDisposition.Disposition, Is.EqualTo (ContentDisposition.Attachment), "Expected Content-Disposition value to be attachment again");

			part.ContentDisposition = null;
			Assert.That (part.ContentDisposition, Is.Null, "Expected ContentDisposition property to be null again");
			Assert.That (part.Headers.Contains (HeaderId.ContentDisposition), Is.False, "Expected header to be removed");

			part.IsAttachment = false;

			Assert.That (part.ContentDisposition, Is.Null, "Expected ContentDisposition property to still be null");
		}

		[Test]
		public void TestContentBase ()
		{
			var relative = new Uri ("relative", UriKind.Relative);
			var uri = new Uri ("http://www.google.com");
			var part = new MimePart ();

			Assert.That (part.ContentBase, Is.Null, "Initial ContentBase should be null");

			Assert.Throws<ArgumentException> (() => part.ContentBase = relative);

			part.ContentBase = uri;
			Assert.That (part.ContentBase, Is.EqualTo (uri), "Expected ContentBase to be updated");
			Assert.That (part.Headers.Contains (HeaderId.ContentBase), Is.True, "Expected header to exist");

			part.ContentBase = null;
			Assert.That (part.ContentBase, Is.Null, "Expected ContentBase to be null again");
			Assert.That (part.Headers.Contains (HeaderId.ContentBase), Is.False, "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentBase, uri.OriginalString);
			Assert.That (part.ContentBase, Is.EqualTo (uri), "Expected ContentBase to be set again");

			part.Headers.Remove (HeaderId.ContentBase);
			Assert.That (part.ContentBase, Is.Null, "Expected ContentBase to be null again");

			part.ContentBase = uri;
			part.Headers.Clear ();
			Assert.That (part.ContentBase, Is.Null, "Expected ContentBase to be null again");
		}

		[Test]
		public void TestContentLocation ()
		{
			var part = new MimePart ();
			var uri = new Uri ("http://www.google.com");

			Assert.That (part.ContentLocation, Is.Null, "Initial ContentLocation should be null");

			part.ContentLocation = uri;
			Assert.That (part.ContentLocation, Is.EqualTo (uri), "Expected ContentLocation to be updated");
			Assert.That (part.Headers.Contains (HeaderId.ContentLocation), Is.True, "Expected header to exist");

			part.ContentLocation = null;
			Assert.That (part.ContentLocation, Is.Null, "Expected ContentLocation to be null again");
			Assert.That (part.Headers.Contains (HeaderId.ContentLocation), Is.False, "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentLocation, uri.OriginalString);
			Assert.That (part.ContentLocation, Is.EqualTo (uri), "Expected ContentLocation to be set again");

			part.Headers.Remove (HeaderId.ContentLocation);
			Assert.That (part.ContentLocation, Is.Null, "Expected ContentLocation to be null again");

			part.ContentLocation = uri;
			part.Headers.Clear ();
			Assert.That (part.ContentLocation, Is.Null, "Expected ContentLocation to be null again");
		}

		[Test]
		public void TestContentDescription ()
		{
			const string description = "This is a sample description.";
			var part = new MimePart ();

			Assert.That (part.ContentDescription, Is.Null, "Initial ContentDescription property should be null");

			part.ContentDescription = description;
			Assert.That (part.ContentDescription, Is.Not.Null, "Expected ContentDescription property to be set");
			Assert.That (part.Headers.Contains (HeaderId.ContentDescription), Is.True, "Expected header to exist");

			part.ContentDescription = null;
			Assert.That (part.ContentDescription, Is.Null, "Expected ContentDescription property to be null again");
			Assert.That (part.Headers.Contains (HeaderId.ContentDescription), Is.False, "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentDescription, description);
			Assert.That (part.ContentDescription, Is.Not.Null, "Expected ContentDescription property to be set again");

			part.Headers.Remove (HeaderId.ContentDescription);
			Assert.That (part.ContentDescription, Is.Null, "Expected ContentDescription to be null again");

			part.ContentDescription = description;
			part.Headers.Clear ();
			Assert.That (part.ContentDescription, Is.Null, "Expected ContentDescription to be null again");
		}

		[Test]
		public void TestContentDuration ()
		{
			var part = new MimePart ();

			Assert.That (part.ContentDuration, Is.Null, "Initial ContentDuration value should be null");

			part.ContentDuration = 500;
			Assert.That (part.ContentDuration, Is.EqualTo (500), "Expected ContentDuration to be updated");
			Assert.That (part.Headers.Contains (HeaderId.ContentDuration), Is.True, "Expected header to exist");

			part.ContentDuration = null;
			Assert.That (part.ContentDuration, Is.Null, "Expected ContentDuration to be null again");
			Assert.That (part.Headers.Contains (HeaderId.ContentDuration), Is.False, "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentDuration, "500");
			Assert.That (part.ContentDuration, Is.EqualTo (500), "Expected ContentDuration to be set again");

			part.Headers.Remove (HeaderId.ContentDuration);
			Assert.That (part.ContentDuration, Is.Null, "Expected ContentDuration to be null again");

			part.ContentDuration = 500;
			part.Headers.Clear ();
			Assert.That (part.ContentDuration, Is.Null, "Expected ContentDuration to be null again");
		}

		[Test]
		public void TestContentId ()
		{
			var id = MimeUtils.GenerateMessageId ();
			var part = new MimePart ();

			Assert.That (part.ContentId, Is.Null, "Initial ContentId value should be null");

			part.ContentId = id;
			Assert.That (part.ContentId, Is.EqualTo (id), "Expected ContentId to be updated");
			Assert.That (part.Headers.Contains (HeaderId.ContentId), Is.True, "Expected header to exist");

			part.ContentId = null;
			Assert.That (part.ContentId, Is.Null, "Expected ContentId to be null again");
			Assert.That (part.Headers.Contains (HeaderId.ContentId), Is.False, "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentId, string.Format ("<{0}>", id));
			Assert.That (part.ContentId, Is.EqualTo (id), "Expected ContentId to be set again");

			part.Headers.Remove (HeaderId.ContentId);
			Assert.That (part.ContentId, Is.Null, "Expected ContentId to be null again");

			part.ContentId = id;
			part.Headers.Clear ();
			Assert.That (part.ContentId, Is.Null, "Expected ContentId to be null again");
		}

		[Test]
		public void TestContentMd5 ()
		{
			var part = new MimePart ();

			Assert.That (part.ContentMd5, Is.Null, "Initial ContentMd5 value should be null");

			part.ContentMd5 = "XYZ";
			Assert.That (part.ContentMd5, Is.EqualTo ("XYZ"), "Expected ContentMd5 to be updated");
			Assert.That (part.Headers.Contains (HeaderId.ContentMd5), Is.True, "Expected header to exist");

			part.ContentMd5 = null;
			Assert.That (part.ContentMd5, Is.Null, "Expected ContentMd5 to be null again");
			Assert.That (part.Headers.Contains (HeaderId.ContentMd5), Is.False, "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentMd5, "XYZ");
			Assert.That (part.ContentMd5, Is.EqualTo ("XYZ"), "Expected ContentMd5 to be set again");

			part.Headers.Remove (HeaderId.ContentMd5);
			Assert.That (part.ContentMd5, Is.Null, "Expected ContentMd5 to be null again");

			part.ContentMd5 = "XYZ";
			part.Headers.Clear ();
			Assert.That (part.ContentMd5, Is.Null, "Expected ContentMd5 to be null again");

			Assert.Throws<InvalidOperationException> (() => part.ComputeContentMd5 ());
			Assert.That (part.VerifyContentMd5 (), Is.False);

			part = new TextPart ("plain") { Text = "Hello, World.\n\nLet's check the MD5 sum of this text!\n" };

			var md5sum = part.ComputeContentMd5 ();

			Assert.That (md5sum, Is.EqualTo ("8criUiOQmpfifOuOmYFtEQ=="), "ComputeContentMd5 text/*");

			// re-encode the base64'd md5sum using a hex encoding so we can easily compare to the output of `md5sum` command-line tools
			var decoded = Convert.FromBase64String (md5sum);
			var encoded = new StringBuilder ();

			for (int i = 0; i < decoded.Length; i++)
				encoded.Append (decoded[i].ToString ("x2"));

			Assert.That (encoded.ToString (), Is.EqualTo ("f1cae25223909a97e27ceb8e99816d11"), "md5sum text/*");

			part.ContentMd5 = md5sum;

			Assert.That (part.VerifyContentMd5 (), Is.True, "VerifyContentMd5");
		}

		[Test]
		public void TestContentTransferEncoding ()
		{
			var part = new MimePart ();

			Assert.That (part.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Default));

			part.ContentTransferEncoding = ContentEncoding.EightBit;
			Assert.That (part.ContentTransferEncoding, Is.EqualTo (ContentEncoding.EightBit));
			Assert.That (part.Headers.Contains (HeaderId.ContentTransferEncoding), Is.True, "Expected header to exist");

			Assert.Throws<ArgumentOutOfRangeException> (() => part.ContentTransferEncoding = (ContentEncoding) 500);

			part.ContentTransferEncoding = ContentEncoding.Default;
			Assert.That (part.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Default));
			Assert.That (part.Headers.Contains (HeaderId.ContentTransferEncoding), Is.False, "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentTransferEncoding, "base64");
			Assert.That (part.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64), "Expected ContentTransferEncoding to be set again");

			part.Headers.Remove (HeaderId.ContentTransferEncoding);
			Assert.That (part.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Default), "Expected ContentTransferEncoding to be default again");

			part.ContentTransferEncoding = ContentEncoding.UUEncode;
			part.Headers.Clear ();
			Assert.That (part.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Default), "Expected ContentTransferEncoding to be default again");
		}

		[Test]
		public void TestPrepare ()
		{
			using (var content = new MemoryStream (new byte[64], false)) {
				var part = new MimePart ("application/octet-stream") {
					Content = new MimeContent (content)
				};

				var encoding = part.GetBestEncoding (EncodingConstraint.SevenBit);

				Assert.That (encoding, Is.EqualTo (ContentEncoding.Base64), "GetBestEncoding");

				part.Prepare (EncodingConstraint.SevenBit);

				Assert.That (part.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64), "Prepare #1");

				// now make sure that calling Prepare() again doesn't change anything

				part.Prepare (EncodingConstraint.SevenBit);

				Assert.That (part.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64), "Prepare #2");

				part.ContentTransferEncoding = ContentEncoding.Binary;
				part.Prepare (EncodingConstraint.None);

				Assert.That (part.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Binary), "Prepare #3");

				part.Prepare (EncodingConstraint.SevenBit);

				Assert.That (part.ContentTransferEncoding, Is.EqualTo (ContentEncoding.Base64), "Prepare #4");
			}
		}

		[Test]
		public void TestTranscoding ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var expected = File.ReadAllBytes (path);

			var part = new MimePart ("image", "jpeg") {
				Content = new MimeContent (new MemoryStream (expected, false)),
				ContentTransferEncoding = ContentEncoding.Base64,
				FileName = "girl.jpg"
			};

			// encode in base64
			using (var output = new MemoryStream ()) {
				part.WriteTo (output);
				output.Position = 0;

				part = (MimePart) MimeEntity.Load (output);
			}

			// transcode to uuencode
			part.ContentTransferEncoding = ContentEncoding.UUEncode;
			using (var output = new MemoryStream ()) {
				part.WriteTo (output);
				output.Position = 0;

				part = (MimePart) MimeEntity.Load (output);
			}

			// verify decoded content
			using (var output = new MemoryStream ()) {
				part.Content.DecodeTo (output);
				output.Position = 0;

				var actual = output.ToArray ();

				Assert.That (actual.Length, Is.EqualTo (expected.Length));
				for (int i = 0; i < expected.Length; i++)
					Assert.That (actual[i], Is.EqualTo (expected[i]), $"Image content differs at index {i}");
			}
		}

		[Test]
		public async Task TestTranscodingAsync ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "images", "girl.jpg");
			var expected = File.ReadAllBytes (path);

			var part = new MimePart ("image", "jpeg") {
				Content = new MimeContent (new MemoryStream (expected, false)),
				ContentTransferEncoding = ContentEncoding.Base64,
				FileName = "girl.jpg"
			};

			// encode in base64
			using (var output = new MemoryStream ()) {
				await part.WriteToAsync (output);
				output.Position = 0;

				part = (MimePart) await MimeEntity.LoadAsync (output);
			}

			// transcode to uuencode
			part.ContentTransferEncoding = ContentEncoding.UUEncode;
			using (var output = new MemoryStream ()) {
				await part.WriteToAsync (output);
				output.Position = 0;

				part = (MimePart) await MimeEntity.LoadAsync (output);
			}

			// verify decoded content
			using (var output = new MemoryStream ()) {
				await part.Content.DecodeToAsync (output);
				output.Position = 0;

				var actual = output.ToArray ();

				Assert.That (actual.Length, Is.EqualTo (expected.Length));
				for (int i = 0; i < expected.Length; i++)
					Assert.That (actual[i], Is.EqualTo (expected[i]), $"Image content differs at index {i}");
			}
		}

		[TestCase ("content", TestName = "TestWriteToNoNewLine")]
		[TestCase ("content\r\n", TestName = "TestWriteToNewLine")]
		public void TestWriteTo (string text)
		{
			var builder = new BodyBuilder ();

			builder.Attachments.Add ("filename", new MemoryStream (Encoding.UTF8.GetBytes (text)));
			builder.TextBody = "This is the text body.";

			var body = builder.ToMessageBody ();

			using (var stream = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				body.WriteTo (options, stream);
				stream.Position = 0;

				var multipart = (Multipart) MimeEntity.Load (stream);
				using (var input = ((MimePart) multipart[1]).Content.Open ()) {
					var buffer = new byte[1024];
					int n;

					n = input.Read (buffer, 0, buffer.Length);

					var content = Encoding.UTF8.GetString (buffer, 0, n);

					Assert.That (content, Is.EqualTo (text));
				}
			}
		}

		[TestCase ("content", TestName = "TestWriteToNoNewLine")]
		[TestCase ("content\r\n", TestName = "TestWriteToNewLine")]
		public async Task TestWriteToAsync (string text)
		{
			var builder = new BodyBuilder ();

			builder.Attachments.Add ("filename", new MemoryStream (Encoding.UTF8.GetBytes (text)));
			builder.TextBody = "This is the text body.";

			var body = builder.ToMessageBody ();

			using (var stream = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				await body.WriteToAsync (options, stream);
				stream.Position = 0;

				var multipart = (Multipart) await MimeEntity.LoadAsync (stream);
				using (var input = ((MimePart) multipart[1]).Content.Open ()) {
					var buffer = new byte[1024];
					int n;

					n = input.Read (buffer, 0, buffer.Length);

					var content = Encoding.UTF8.GetString (buffer, 0, n);

					Assert.That (content, Is.EqualTo (text));
				}
			}
		}

		[Test]
		public void TestLoadHttpWebResponse ()
		{
			var text = "This is some text and stuff." + Environment.NewLine;
			var contentType = new ContentType ("text", "plain");

			using (var content = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var entity = MimeEntity.Load (contentType, content);

				Assert.That (entity, Is.InstanceOf<TextPart> ());

				var part = (TextPart) entity;

				Assert.That (part.Text, Is.EqualTo (text));
			}
		}

		[Test]
		public async Task TestLoadHttpWebResponseAsync ()
		{
			var text = "This is some text and stuff." + Environment.NewLine;
			var contentType = new ContentType ("text", "plain");

			using (var content = new MemoryStream (Encoding.ASCII.GetBytes (text), false)) {
				var entity = await MimeEntity.LoadAsync (contentType, content);

				Assert.That (entity, Is.InstanceOf<TextPart> ());

				var part = (TextPart) entity;

				Assert.That (part.Text, Is.EqualTo (text));
			}
		}

		[Test]
		public void TestToString ()
		{
			var part = new MimePart ("application", "octet-stream") {
				ContentTransferEncoding = ContentEncoding.Base64,
				Content = new MimeContent (new MemoryStream (new byte[1], false))
			};

			try {
				part.ToString ();
			} catch (Exception ex) {
				Assert.Fail (ex.Message);
			}
		}
	}
}
