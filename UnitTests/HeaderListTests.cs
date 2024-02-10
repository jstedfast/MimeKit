//
// HeaderListTests.cs
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

namespace UnitTests {
	[TestFixture]
	public class HeaderListTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var list = new HeaderList ();
			Header header;
			string value;

			using (var stream = new MemoryStream ()) {
				Assert.Throws<ArgumentNullException> (() => HeaderList.Load (null, "filename.txt"));
				Assert.Throws<ArgumentNullException> (() => HeaderList.Load (ParserOptions.Default, (string) null));

				Assert.Throws<ArgumentNullException> (() => HeaderList.Load (null, stream));
				Assert.Throws<ArgumentNullException> (() => HeaderList.Load (ParserOptions.Default, (Stream) null));

				Assert.ThrowsAsync<ArgumentNullException> (async () => await HeaderList.LoadAsync (null, "filename.txt"));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await HeaderList.LoadAsync (ParserOptions.Default, (string) null));

				Assert.ThrowsAsync<ArgumentNullException> (async () => await HeaderList.LoadAsync (null, stream));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await HeaderList.LoadAsync (ParserOptions.Default, (Stream) null));
			}

			// Add
			Assert.Throws<ArgumentNullException> (() => list.Add (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Add (HeaderId.Unknown, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Add (HeaderId.AdHoc, null));
			Assert.Throws<ArgumentNullException> (() => list.Add (null, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Add ("field", null));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Add (HeaderId.Unknown, Encoding.UTF8, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Add (HeaderId.AdHoc, null, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Add (HeaderId.AdHoc, Encoding.UTF8, null));
			Assert.Throws<ArgumentNullException> (() => list.Add (null, Encoding.UTF8, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Add ("field", null, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Add ("field", Encoding.UTF8, null));

			// Contains
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Contains (HeaderId.Unknown));
			Assert.Throws<ArgumentNullException> (() => list.Contains ((Header) null));
			Assert.Throws<ArgumentNullException> (() => list.Contains ((string) null));

			// CopyTo
			Assert.Throws<ArgumentOutOfRangeException> (() => list.CopyTo (Array.Empty<Header> (), -1));
			Assert.Throws<ArgumentNullException> (() => list.CopyTo (null, 0));

			// IndexOf
			Assert.Throws<ArgumentOutOfRangeException> (() => list.IndexOf (HeaderId.Unknown));
			Assert.Throws<ArgumentNullException> (() => list.IndexOf ((Header) null));
			Assert.Throws<ArgumentNullException> (() => list.IndexOf ((string) null));

			// Insert
			list.Add ("field", "value");
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (-1, new Header (HeaderId.AdHoc, "value")));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (-1, HeaderId.AdHoc, Encoding.UTF8, "value"));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (-1, "field", Encoding.UTF8, "value"));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (-1, HeaderId.AdHoc, "value"));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (-1, "field", "value"));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (0, HeaderId.Unknown, Encoding.UTF8, "value"));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Insert (0, HeaderId.Unknown, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, HeaderId.AdHoc, Encoding.UTF8, null));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, HeaderId.AdHoc, null, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, HeaderId.AdHoc, null));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, null, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, "field", null));
			Assert.Throws<ArgumentNullException> (() => list.Insert (0, null));

			// LastIndexOf
			Assert.Throws<ArgumentOutOfRangeException> (() => list.LastIndexOf (HeaderId.Unknown));
			Assert.Throws<ArgumentNullException> (() => list.LastIndexOf ((string) null));

			// Remove
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Remove (HeaderId.Unknown));
			Assert.Throws<ArgumentNullException> (() => list.Remove ((Header) null));
			Assert.Throws<ArgumentNullException> (() => list.Remove ((string) null));

			// RemoveAll
			Assert.Throws<ArgumentOutOfRangeException> (() => list.RemoveAll (HeaderId.Unknown));
			Assert.Throws<ArgumentNullException> (() => list.RemoveAll ((string) null));

			// RemoveAt
			Assert.Throws<ArgumentOutOfRangeException> (() => list.RemoveAt (-1));

			// Replace
			Assert.Throws<ArgumentNullException> (() => list.Replace (null));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Replace (HeaderId.Unknown, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Replace (HeaderId.AdHoc, null));
			Assert.Throws<ArgumentNullException> (() => list.Replace (null, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Replace ("field", null));
			Assert.Throws<ArgumentOutOfRangeException> (() => list.Replace (HeaderId.Unknown, Encoding.UTF8, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Replace (HeaderId.AdHoc, null, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Replace (HeaderId.AdHoc, Encoding.UTF8, null));
			Assert.Throws<ArgumentNullException> (() => list.Replace (null, Encoding.UTF8, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Replace ("field", null, "value"));
			Assert.Throws<ArgumentNullException> (() => list.Replace ("field", Encoding.UTF8, null));

			using (var stream = new MemoryStream ()) {
				// Load
				Assert.Throws<ArgumentNullException> (() => HeaderList.Load (ParserOptions.Default, (Stream) null));
				Assert.Throws<ArgumentNullException> (() => HeaderList.Load (ParserOptions.Default, (string) null));
				Assert.Throws<ArgumentNullException> (() => HeaderList.Load (null, stream));
				Assert.Throws<ArgumentNullException> (() => HeaderList.Load ((Stream) null));
				Assert.Throws<ArgumentNullException> (() => HeaderList.Load ((string) null));

				// LoadAsync
				Assert.ThrowsAsync<ArgumentNullException> (async () => await HeaderList.LoadAsync (ParserOptions.Default, (Stream) null));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await HeaderList.LoadAsync (ParserOptions.Default, (string) null));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await HeaderList.LoadAsync (null, stream));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await HeaderList.LoadAsync ((Stream) null));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await HeaderList.LoadAsync ((string) null));

				// WriteTo
				Assert.Throws<ArgumentNullException> (() => list.WriteTo (FormatOptions.Default, null));
				Assert.Throws<ArgumentNullException> (() => list.WriteTo (null, stream));
				Assert.Throws<ArgumentNullException> (() => list.WriteTo (null));

				// WriteToAsync
				Assert.ThrowsAsync<ArgumentNullException> (async () => await list.WriteToAsync (FormatOptions.Default, null));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await list.WriteToAsync (null, stream));
				Assert.ThrowsAsync<ArgumentNullException> (async () => await list.WriteToAsync (null));
			}

			// Indexers
			Assert.Throws<ArgumentOutOfRangeException> (() => list[-1] = new Header (HeaderId.AdHoc, "value"));
			Assert.Throws<ArgumentOutOfRangeException> (() => list[HeaderId.Unknown] = "value");
			Assert.Throws<ArgumentOutOfRangeException> (() => value = list[HeaderId.Unknown]);
			Assert.Throws<ArgumentOutOfRangeException> (() => header = list[-1]);
			Assert.Throws<ArgumentNullException> (() => list[HeaderId.AdHoc] = null);
			Assert.Throws<ArgumentNullException> (() => value = list[null]);
			Assert.Throws<ArgumentNullException> (() => list[null] = "value");
			Assert.Throws<ArgumentNullException> (() => list["field"] = null);
			Assert.Throws<ArgumentNullException> (() => list[0] = null);
		}

		[Test]
		public void TestRemovingHeaders ()
		{
			var headers = new HeaderList {
				{ "From", "sender@localhost" },
				{ "To", "first@localhost" },
				{ "To", "second@localhost" },
				{ "To", "third@localhost" },
				{ "To", "fourth@localhost" },
				{ "Cc", "carbon.copy@localhost" }
			};

			Assert.That (headers.IsReadOnly, Is.False);
			Assert.That (headers.Contains (new Header (HeaderId.Received, "value")), Is.False);
			Assert.That (headers.IndexOf (new Header (HeaderId.Received, "value")), Is.EqualTo (-1));
			Assert.That (headers.IndexOf ("Received"), Is.EqualTo (-1));
			Assert.That (headers.LastIndexOf (HeaderId.Received), Is.EqualTo (-1));
			Assert.That (headers[HeaderId.Received], Is.EqualTo (null));

			Assert.That (headers.Remove ("Cc"), Is.True);

			// try removing a header that no longer exists
			Assert.That (headers.Remove (new Header (HeaderId.Cc, "value")), Is.False);
			Assert.That (headers.Remove (HeaderId.Cc), Is.False);
			Assert.That (headers.Remove ("Cc"), Is.False);

			// removing this will change the result of headers[HeaderId.To]
			Assert.That (headers[HeaderId.To], Is.EqualTo ("first@localhost"));
			Assert.That (headers.Remove (HeaderId.To), Is.True);
			Assert.That (headers[HeaderId.To], Is.EqualTo ("second@localhost"));
			Assert.That (headers.Remove ("To"), Is.True);
			Assert.That (headers[HeaderId.To], Is.EqualTo ("third@localhost"));
			headers.RemoveAt (headers.IndexOf ("To"));
			Assert.That (headers[HeaderId.To], Is.EqualTo ("fourth@localhost"));
		}

		[Test]
		public void TestReplacingHeaders ()
		{
			const string ReplacedContentType = "text/plain; charset=iso-8859-1; name=body.txt";
			const string ReplacedContentDisposition = "inline; filename=body.txt";
			const string ReplacedContentLocation = "http://www.example.com/location";
			const string ReplacedContentId = "<content.id.2@localhost>";
			var headers = new HeaderList {
				{ HeaderId.ContentId, "<content-id.1@localhost>" },
				{ "Content-Location", "http://www.location.com" }
			};

			headers.Insert (0, HeaderId.ContentDisposition, "attachment");
			headers.Insert (0, "Content-Type", "text/plain");

			Assert.That (headers.Contains (HeaderId.ContentType), Is.True, "Expected the list of headers to contain HeaderId.ContentType.");
			Assert.That (headers.Contains ("Content-Type"), Is.True, "Expected the list of headers to contain a Content-Type header.");
			Assert.That (headers.LastIndexOf (HeaderId.ContentType), Is.EqualTo (0), "Expected the Content-Type header to be the first header.");

			headers.Replace ("Content-Disposition", ReplacedContentDisposition);
			Assert.That (headers.Count, Is.EqualTo (4), "Unexpected number of headers after replacing Content-Disposition.");
			Assert.That (headers["Content-Disposition"], Is.EqualTo (ReplacedContentDisposition), "Content-Disposition has unexpected value after replacing it.");
			Assert.That (headers.IndexOf ("Content-Disposition"), Is.EqualTo (1), "Replaced Content-Disposition not in the expected position.");

			headers.Replace (HeaderId.ContentType, ReplacedContentType);
			Assert.That (headers.Count, Is.EqualTo (4), "Unexpected number of headers after replacing Content-Type.");
			Assert.That (headers["Content-Type"], Is.EqualTo (ReplacedContentType), "Content-Type has unexpected value after replacing it.");
			Assert.That (headers.IndexOf ("Content-Type"), Is.EqualTo (0), "Replaced Content-Type not in the expected position.");

			headers.Replace (HeaderId.ContentId, Encoding.UTF8, ReplacedContentId);
			Assert.That (headers.Count, Is.EqualTo (4), "Unexpected number of headers after replacing Content-Id.");
			Assert.That (headers["Content-Id"], Is.EqualTo (ReplacedContentId), "Content-Id has unexpected value after replacing it.");
			Assert.That (headers.IndexOf ("Content-Id"), Is.EqualTo (2), "Replaced Content-Id not in the expected position.");

			headers.Replace ("Content-Location", Encoding.UTF8, ReplacedContentLocation);
			Assert.That (headers.Count, Is.EqualTo (4), "Unexpected number of headers after replacing Content-Location.");
			Assert.That (headers["Content-Location"], Is.EqualTo (ReplacedContentLocation), "Content-Location has unexpected value after replacing it.");
			Assert.That (headers.IndexOf ("Content-Location"), Is.EqualTo (3), "Replaced Content-Location not in the expected position.");

			headers.RemoveAll ("Content-Location");
			Assert.That (headers.Count, Is.EqualTo (3), "Unexpected number of headers after removing Content-Location.");

			headers.Clear ();

			headers.Add (HeaderId.Received, "received 1");
			headers.Add (HeaderId.Received, "received 2");
			headers.Add (HeaderId.Received, "received 3");
			headers.Add (HeaderId.ReturnPath, "return-path");

			headers[0] = new Header (HeaderId.ReturnPath, "new return-path");
			Assert.That (headers[HeaderId.ReturnPath], Is.EqualTo ("new return-path"));
			headers[0] = new Header (HeaderId.Received, "new received");
			Assert.That (headers[HeaderId.Received], Is.EqualTo ("new received"));
		}

		[Test]
		public void TestReplacingMultipleHeaders ()
		{
			const string CombinedRecpients = "first@localhost, second@localhost, third@localhost";
			var headers = new HeaderList {
				{ "From", "sender@localhost" },
				{ "To", "first@localhost" },
				{ "To", "second@localhost" },
				{ "To", "third@localhost" },
				{ "Cc", "carbon.copy@localhost" }
			};

			headers.Replace ("To", CombinedRecpients);
			Assert.That (headers.Count, Is.EqualTo (3), "Unexpected number of headers after replacing To header.");
			Assert.That (headers["To"], Is.EqualTo (CombinedRecpients), "To header has unexpected value after being replaced.");
			Assert.That (headers.IndexOf ("To"), Is.EqualTo (1), "Replaced To header not in the expected position.");
			Assert.That (headers.IndexOf ("From"), Is.EqualTo (0), "From header not in the expected position.");
			Assert.That (headers.IndexOf ("Cc"), Is.EqualTo (2), "Cc header not in the expected position.");
		}
	}
}
