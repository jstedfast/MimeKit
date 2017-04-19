//
// AttachmentCollectionTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((string) null));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((MimeEntity) null));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((string) null, data));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((string) null, stream));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((string) null, data, new ContentType ("application", "octet-stream")));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ((string) null, stream, new ContentType ("application", "octet-stream")));

				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", (byte[]) null));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", (Stream) null));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", (byte[]) null, new ContentType ("application", "octet-stream")));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", (Stream) null, new ContentType ("application", "octet-stream")));

				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", data, null));
				Assert.Throws<ArgumentNullException> (() => attachments.Add ("file.dat", stream, null));

				Assert.Throws<ArgumentNullException> (() => attachments.Contains (null));

				Assert.Throws<ArgumentNullException> (() => attachments.CopyTo (null, 0));
				Assert.Throws<ArgumentOutOfRangeException> (() => attachments.CopyTo (items, -1));

				Assert.Throws<ArgumentNullException> (() => attachments.IndexOf (null));

				Assert.Throws<ArgumentNullException> (() => attachments.Remove (null));
				Assert.Throws<ArgumentOutOfRangeException> (() => attachments.RemoveAt (0));

				Assert.Throws<ArgumentOutOfRangeException> (() => { var x = attachments[10]; });
				Assert.Throws<ArgumentOutOfRangeException> (() => attachments[10] = new TextPart ("plain"));
			}
		}
	}
}
