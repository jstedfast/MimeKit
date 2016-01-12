//
// HeaderListTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2016 Xamarin Inc. (www.xamarin.com)
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
using System.Text;
using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class HeaderListTests
	{
		[Test]
		public void TestReplacingHeaders ()
		{
			const string ReplacedContentType = "text/plain; charset=iso-8859-1; name=body.txt";
			const string ReplacedContentDisposition = "inline; filename=body.txt";
			const string ReplacedContentLocation = "http://www.example.com/location";
			const string ReplacedContentId = "<content.id.2@localhost>";
			var headers = new HeaderList ();

			headers.Add (HeaderId.ContentDisposition, "attachment");
			headers.Add ("Content-Id", "<content-id.1@localhost>");
			headers.Add ("Content-Location", "C:\\location");
			headers.Insert (0, "Content-Type", "text/plain");

			Assert.IsTrue (headers.Contains (HeaderId.ContentType), "Expected the list of headers to contain HeaderId.ContentType.");
			Assert.IsTrue (headers.Contains ("Content-Type"), "Expected the list of headers to contain a Content-Type header.");
			Assert.AreEqual (0, headers.LastIndexOf (HeaderId.ContentType), "Expected the Content-Type header to be the first header.");

			headers.Replace ("Content-Disposition", ReplacedContentDisposition);
			Assert.AreEqual (4, headers.Count, "Unexpected number of headers after replacing Content-Disposition.");
			Assert.AreEqual (ReplacedContentDisposition, headers["Content-Disposition"], "Content-Disposition has unexpected value after replacing it.");
			Assert.AreEqual (1, headers.IndexOf ("Content-Disposition"), "Replaced Content-Disposition not in the expected position.");

			headers.Replace (HeaderId.ContentType, ReplacedContentType);
			Assert.AreEqual (4, headers.Count, "Unexpected number of headers after replacing Content-Type.");
			Assert.AreEqual (ReplacedContentType, headers["Content-Type"], "Content-Type has unexpected value after replacing it.");
			Assert.AreEqual (0, headers.IndexOf ("Content-Type"), "Replaced Content-Type not in the expected position.");

			headers.Replace (HeaderId.ContentId, Encoding.UTF8, ReplacedContentId);
			Assert.AreEqual (4, headers.Count, "Unexpected number of headers after replacing Content-Id.");
			Assert.AreEqual (ReplacedContentId, headers["Content-Id"], "Content-Id has unexpected value after replacing it.");
			Assert.AreEqual (2, headers.IndexOf ("Content-Id"), "Replaced Content-Id not in the expected position.");

			headers.Replace ("Content-Location", Encoding.UTF8, ReplacedContentLocation);
			Assert.AreEqual (4, headers.Count, "Unexpected number of headers after replacing Content-Location.");
			Assert.AreEqual (ReplacedContentLocation, headers["Content-Location"], "Content-Location has unexpected value after replacing it.");
			Assert.AreEqual (3, headers.IndexOf ("Content-Location"), "Replaced Content-Location not in the expected position.");

			headers.RemoveAll ("Content-Location");
			Assert.AreEqual (3, headers.Count, "Unexpected number of headers after removing Content-Location.");
		}

		[Test]
		public void TestReplacingMultipleHeaders ()
		{
			const string CombinedRecpients = "first@localhost, second@localhost, third@localhost";
			var headers = new HeaderList ();

			headers.Add ("From", "sender@localhost");
			headers.Add ("To", "first@localhost");
			headers.Add ("To", "second@localhost");
			headers.Add ("To", "third@localhost");
			headers.Add ("Cc", "carbon.copy@localhost");

			headers.Replace ("To", CombinedRecpients);
			Assert.AreEqual (3, headers.Count, "Unexpected number of headers after replacing To header.");
			Assert.AreEqual (CombinedRecpients, headers["To"], "To header has unexpected value after being replaced.");
			Assert.AreEqual (1, headers.IndexOf ("To"), "Replaced To header not in the expected position.");
			Assert.AreEqual (0, headers.IndexOf ("From"), "From header not in the expected position.");
			Assert.AreEqual (2, headers.IndexOf ("Cc"), "Cc header not in the expected position.");
		}
	}
}
