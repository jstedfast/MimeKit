//
// FilteredStreamTests.cs
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
using MimeKit.IO;
using MimeKit.IO.Filters;

namespace UnitTests.IO {
	[TestFixture]
	public class FilteredStreamTests
	{
		[Test]
		public void TestCanReadWriteSeekTimeout ()
		{
			var buffer = new byte[1024];

			using (var filtered = new FilteredStream (new CanReadWriteSeekStream (true, false, false, false))) {
				Assert.IsTrue (filtered.CanRead);
				Assert.IsFalse (filtered.CanWrite);
				Assert.IsFalse (filtered.CanSeek);
				Assert.IsFalse (filtered.CanTimeout);

				Assert.Throws<NotImplementedException> (() => filtered.Read (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => filtered.Write (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => filtered.Seek (0, SeekOrigin.End));
			}

			using (var filtered = new FilteredStream (new CanReadWriteSeekStream (false, true, false, false))) {
				Assert.IsFalse (filtered.CanRead);
				Assert.IsTrue (filtered.CanWrite);
				Assert.IsFalse (filtered.CanSeek);
				Assert.IsFalse (filtered.CanTimeout);

				Assert.Throws<NotSupportedException> (() => filtered.Read (buffer, 0, buffer.Length));
				Assert.Throws<NotImplementedException> (() => filtered.Write (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => filtered.Seek (0, SeekOrigin.End));
			}

			using (var filtered = new FilteredStream (new CanReadWriteSeekStream (false, false, true, false))) {
				Assert.IsFalse (filtered.CanRead);
				Assert.IsFalse (filtered.CanWrite);
				Assert.IsFalse (filtered.CanSeek);  // FilteredStream can never seek
				Assert.IsFalse (filtered.CanTimeout);

				Assert.Throws<NotSupportedException> (() => filtered.Read (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => filtered.Write (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => filtered.Seek (0, SeekOrigin.End)); // FilteredStream can never seek
			}
		}

		[Test]
		public void TestGetSetTimeouts ()
		{
			using (var filtered = new FilteredStream (new TimeoutStream ())) {
				Assert.AreEqual (0, filtered.ReadTimeout);
				Assert.AreEqual (0, filtered.WriteTimeout);

				filtered.ReadTimeout = 10;
				Assert.AreEqual (10, filtered.ReadTimeout);

				filtered.WriteTimeout = 100;
				Assert.AreEqual (100, filtered.WriteTimeout);
			}
		}

		[Test]
		public void TestRead ()
		{
			using (var original = new MemoryStream ()) {
				using (var file = File.OpenRead ("../../TestData/encoders/photo.jpg"))
					file.CopyTo (original, 4096);

				using (var decoded = new MemoryStream ()) {
					using (var file = File.OpenRead ("../../TestData/encoders/photo.b64")) {
						using (var filtered = new FilteredStream (file)) {
							filtered.Add (DecoderFilter.Create (ContentEncoding.Base64));
							filtered.CopyTo (decoded, 4096);
						}
					}

					var buf0 = original.GetBuffer ();
					var buf1 = decoded.GetBuffer ();
					int n = (int) original.Length;

					Assert.AreEqual (original.Length, decoded.Length, "Decoded length is incorrect.");

					for (int i = 0; i < n; i++)
						Assert.AreEqual (buf0[i], buf1[i], "The byte at offset {0} does not match.", i);
				}
			}
		}

		[Test]
		public async void TestReadAsync ()
		{
			using (var original = new MemoryStream ()) {
				using (var file = File.OpenRead ("../../TestData/encoders/photo.jpg"))
					file.CopyTo (original, 4096);

				using (var decoded = new MemoryStream ()) {
					using (var file = File.OpenRead ("../../TestData/encoders/photo.b64")) {
						using (var filtered = new FilteredStream (file)) {
							filtered.Add (DecoderFilter.Create (ContentEncoding.Base64));
							await filtered.CopyToAsync (decoded, 4096);
						}
					}

					var buf0 = original.GetBuffer ();
					var buf1 = decoded.GetBuffer ();
					int n = (int) original.Length;

					Assert.AreEqual (original.Length, decoded.Length, "Decoded length is incorrect.");

					for (int i = 0; i < n; i++)
						Assert.AreEqual (buf0[i], buf1[i], "The byte at offset {0} does not match.", i);
				}
			}
		}

		[Test]
		public void TestWrite ()
		{
			using (var original = new MemoryStream ()) {
				using (var file = File.OpenRead ("../../TestData/encoders/photo.jpg"))
					file.CopyTo (original, 4096);

				using (var decoded = new MemoryStream ()) {
					using (var file = File.OpenRead ("../../TestData/encoders/photo.b64")) {
						using (var filtered = new FilteredStream (decoded)) {
							filtered.Add (DecoderFilter.Create (ContentEncoding.Base64));
							file.CopyTo (filtered, 4096);
							filtered.Flush ();
						}
					}

					var buf0 = original.GetBuffer ();
					var buf1 = decoded.GetBuffer ();
					int n = (int) original.Length;

					Assert.AreEqual (original.Length, decoded.Length, "Decoded length is incorrect.");

					for (int i = 0; i < n; i++)
						Assert.AreEqual (buf0[i], buf1[i], "The byte at offset {0} does not match.", i);
				}
			}
		}

		[Test]
		public async void TestWriteAsync ()
		{
			using (var original = new MemoryStream ()) {
				using (var file = File.OpenRead ("../../TestData/encoders/photo.jpg"))
					file.CopyTo (original, 4096);

				using (var decoded = new MemoryStream ()) {
					using (var file = File.OpenRead ("../../TestData/encoders/photo.b64")) {
						using (var filtered = new FilteredStream (decoded)) {
							filtered.Add (DecoderFilter.Create (ContentEncoding.Base64));
							await file.CopyToAsync (filtered, 4096);
							await filtered.FlushAsync ();
						}
					}

					var buf0 = original.GetBuffer ();
					var buf1 = decoded.GetBuffer ();
					int n = (int) original.Length;

					Assert.AreEqual (original.Length, decoded.Length, "Decoded length is incorrect.");

					for (int i = 0; i < n; i++)
						Assert.AreEqual (buf0[i], buf1[i], "The byte at offset {0} does not match.", i);
				}
			}
		}

		[Test]
		public void TestSetLength ()
		{
			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					Assert.Throws<NotSupportedException> (() => filtered.SetLength (500));
				}
			}
		}
	}
}
