//
// MimePartTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Jeffrey Stedfast
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

			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync ((Stream) null));
			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync ((Stream) null, true));
			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync ((ParserOptions) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync (ParserOptions.Default, (Stream) null));
			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync (null, Stream.Null, true));
			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync (ParserOptions.Default, (Stream) null, true));

			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync ((ContentType) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync (new ContentType ("application", "octet-stream"), null));
			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync (null, new ContentType ("application", "octet-stream"), Stream.Null));
			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync (ParserOptions.Default, null, Stream.Null));
			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync (ParserOptions.Default, new ContentType ("application", "octet-stream"), null));

			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync ((string) null));
			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync (null, "fileName"));
			Assert.Throws<ArgumentNullException> (async () => await MimeEntity.LoadAsync (ParserOptions.Default, (string) null));

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

			Assert.Throws<ArgumentNullException> (async () => await part.WriteToAsync ((string) null));
			Assert.Throws<ArgumentNullException> (async () => await part.WriteToAsync ((Stream) null));
			Assert.Throws<ArgumentNullException> (async () => await part.WriteToAsync ((string) null, false));
			Assert.Throws<ArgumentNullException> (async () => await part.WriteToAsync ((Stream) null, false));
			Assert.Throws<ArgumentNullException> (async () => await part.WriteToAsync (null, Stream.Null));
			Assert.Throws<ArgumentNullException> (async () => await part.WriteToAsync (FormatOptions.Default, (Stream) null));
			Assert.Throws<ArgumentNullException> (async () => await part.WriteToAsync (null, "fileName"));
			Assert.Throws<ArgumentNullException> (async () => await part.WriteToAsync (FormatOptions.Default, (string) null));
			Assert.Throws<ArgumentNullException> (async () => await part.WriteToAsync (null, Stream.Null, false));
			Assert.Throws<ArgumentNullException> (async () => await part.WriteToAsync (FormatOptions.Default, (Stream) null, false));
			Assert.Throws<ArgumentNullException> (async () => await part.WriteToAsync (null, "fileName", false));
			Assert.Throws<ArgumentNullException> (async () => await part.WriteToAsync (FormatOptions.Default, (string) null, false));
		}

		[Test]
		public void TestContentDisposition ()
		{
			var part = new MimePart ();

			Assert.IsNull (part.ContentDisposition, "Initial ContentDisposition should be null");

			part.ContentDisposition = new ContentDisposition (ContentDisposition.Attachment);
			Assert.IsNotNull (part.ContentDisposition, "Expected ContentDisposition to be set");
			Assert.IsTrue (part.Headers.Contains (HeaderId.ContentDisposition), "Expected header to exist");

			part.ContentDisposition = null;
			Assert.IsNull (part.ContentDisposition, "Expected ContentDisposition to be null again");
			Assert.IsFalse (part.Headers.Contains (HeaderId.ContentDisposition), "Expected header to be removed");

			part.Headers.Add (HeaderId.ContentDisposition, "attachment");
			Assert.IsNotNull (part.ContentDisposition, "Expected ContentDisposition to be set again");

			part.Headers.Remove (HeaderId.ContentDisposition);
			Assert.IsNull (part.ContentBase, "Expected ContentDisposition to be null again");

			part.ContentDisposition = new ContentDisposition ();
			part.FileName = "fileName";
			part.Headers.Clear ();
			Assert.IsNull (part.ContentBase, "Expected ContentDisposition to be null again");
		}

		[Test]
		public void TestContentBase ()
		{
			var relative = new Uri ("relative", UriKind.Relative);
			var uri = new Uri ("http://www.google.com");
			var part = new MimePart ();

			Assert.IsNull (part.ContentBase, "Initial ContentBase should be null");

			Assert.Throws<ArgumentException> (() => part.ContentBase = relative);

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

			part.ContentBase = uri;
			part.Headers.Clear ();
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

			part.ContentLocation = uri;
			part.Headers.Clear ();
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

			part.ContentDuration = 500;
			part.Headers.Clear ();
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

			part.ContentId = id;
			part.Headers.Clear ();
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

			part.ContentMd5 = "XYZ";
			part.Headers.Clear ();
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

			part.ContentTransferEncoding = ContentEncoding.UUEncode;
			part.Headers.Clear ();
			Assert.AreEqual (ContentEncoding.Default, part.ContentTransferEncoding, "Expected ContentTransferEncoding to be default again");
		}

		[Test]
		public void TestTranscoding ()
		{
			var path = Path.Combine ("..", "..", "TestData", "images", "girl.jpg");
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

				Assert.AreEqual (expected.Length, actual.Length);
				for (int i = 0; i < expected.Length; i++)
					Assert.AreEqual (expected[i], actual[i], "Image content differs at index {0}", i);
			}
		}

		[Test]
		public async void TestTranscodingAsync ()
		{
			var path = Path.Combine ("..", "..", "TestData", "images", "girl.jpg");
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

				Assert.AreEqual (expected.Length, actual.Length);
				for (int i = 0; i < expected.Length; i++)
					Assert.AreEqual (expected[i], actual[i], "Image content differs at index {0}", i);
			}
		}

		[TestCase ("content", TestName = "TestWriteToNoNewLine")]
		[TestCase ("content\r\n", TestName = "TestWriteToNewLine")]
		public void TestWriteTo (string text)
		{
			var builder = new BodyBuilder ();

			builder.Attachments.Add ("filename", new MemoryStream (Encoding.UTF8.GetBytes (text)));

			var body = builder.ToMessageBody ();

			using (var stream = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				body.WriteTo (options, stream);
				stream.Position = 0;

				var multipart = (Multipart) MimeEntity.Load (stream);
				using (var input = ((MimePart) multipart[0]).Content.Open ()) {
					var buffer = new byte[1024];
					int n;

					n = input.Read (buffer, 0, buffer.Length);

					var content = Encoding.UTF8.GetString (buffer, 0, n);

					Assert.AreEqual (text, content);
				}
			}
		}

		[TestCase ("content", TestName = "TestWriteToNoNewLine")]
		[TestCase ("content\r\n", TestName = "TestWriteToNewLine")]
		public async void TestWriteToAsync (string text)
		{
			var builder = new BodyBuilder ();

			builder.Attachments.Add ("filename", new MemoryStream (Encoding.UTF8.GetBytes (text)));

			var body = builder.ToMessageBody ();

			using (var stream = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				await body.WriteToAsync (options, stream);
				stream.Position = 0;

				var multipart = (Multipart) await MimeEntity.LoadAsync (stream);
				using (var input = ((MimePart) multipart[0]).Content.Open ()) {
					var buffer = new byte[1024];
					int n;

					n = input.Read (buffer, 0, buffer.Length);

					var content = Encoding.UTF8.GetString (buffer, 0, n);

					Assert.AreEqual (text, content);
				}
			}
		}
	}
}
