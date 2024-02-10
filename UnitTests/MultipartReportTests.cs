//
// MultipartReportTests.cs
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

namespace UnitTests {
	[TestFixture]
	public class MultipartReportTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var report = new MultipartReport ("disposition-notification");

			Assert.Throws<ArgumentNullException> (() => new MultipartReport ((MimeEntityConstructorArgs) null));
			Assert.Throws<ArgumentNullException> (() => new MultipartReport ((string) null));
			Assert.Throws<ArgumentNullException> (() => new MultipartReport (null, Array.Empty<object> ()));
			Assert.Throws<ArgumentNullException> (() => new MultipartReport ("disposition-notification", null));

			Assert.Throws<ArgumentNullException> (() => report.ReportType = null);

			Assert.Throws<ArgumentNullException> (() => report.Accept (null));
		}

		[Test]
		public void TestGenericArgsConstructor ()
		{
			var multipart = new MultipartReport ("disposition-notification",
				new Header (HeaderId.ContentDescription, "This is a description of the multipart."),
				new TextPart (TextFormat.Plain) { Text = "This is the message body." },
				new MimePart ("image", "gif") { FileName = "attachment.gif" }
				);

			Assert.That (multipart.ReportType, Is.EqualTo ("disposition-notification"), "ReportType");
			Assert.That (multipart.Headers.Contains (HeaderId.ContentDescription), Is.True, "Content-Description header");
			Assert.That (multipart.Count, Is.EqualTo (2), "Child part count");
			Assert.That (multipart[0].ContentType.MimeType, Is.EqualTo ("text/plain"), "MimeType[0]");
			Assert.That (multipart[1].ContentType.MimeType, Is.EqualTo ("image/gif"), "MimeType[1]");
		}
	}
}
