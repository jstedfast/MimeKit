//
// MemoryBlockStreamTests.cs
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

using MimeKit.IO;

namespace UnitTests.IO {
	[TestFixture]
	public class MemoryBlockStreamTests
	{
		MemoryBlockStream blocks;
		MemoryStream master;
		byte[] buf, mbuf;
		Random random;

		public MemoryBlockStreamTests ()
		{
			var bytes = new byte[9 * 1024];
			int position = 0;

			random = new Random ();
			random.NextBytes (bytes);

			// this is our master stream, all operations on the chained stream
			// should match the results on this stream
			master = new MemoryStream (bytes);
			mbuf = new byte[4096];
			buf = new byte[4096];

			// write the content into the memory block stream in random chunks
			blocks = new MemoryBlockStream ();

			while (position < bytes.Length) {
				int n = Math.Min (bytes.Length - position, random.Next () % 4096);
				blocks.Write (bytes, position, n);
				position += n;
			}

			blocks.Seek (0, SeekOrigin.Begin);
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			blocks.Dispose ();
			master.Dispose ();
		}

		[Test]
		public void TestCanReadWriteSeek ()
		{
			var buffer = new byte[1024];

			using (var block = new MemoryBlockStream ()) {
				Assert.IsTrue (block.CanRead);
				Assert.IsTrue (block.CanWrite);
				Assert.IsTrue (block.CanSeek);
				Assert.IsFalse (block.CanTimeout);
			}
		}

		[Test]
		public void TestGetSetTimeouts ()
		{
			using (var block = new MemoryBlockStream ()) {
				Assert.Throws<InvalidOperationException> (() => { int x = block.ReadTimeout; });
				Assert.Throws<InvalidOperationException> (() => { int x = block.WriteTimeout; });

				Assert.Throws<InvalidOperationException> (() => block.ReadTimeout = 5);
				Assert.Throws<InvalidOperationException> (() => block.WriteTimeout = 5);
			}
		}

		[Test]
		public void TestRead ()
		{
			blocks.Position = 0;
			master.Position = 0;

			do {
				int nread = blocks.Read (buf, 0, buf.Length);
				int mread = master.Read (mbuf, 0, mbuf.Length);

				Assert.AreEqual (mread, nread, "Did not read the expected number of bytes from the memory block stream");
				Assert.AreEqual (master.Position, blocks.Position, "The memory block stream's position did not match");

				for (int i = 0; i < mread; i++)
					Assert.AreEqual (mbuf[i], buf[i], "The bytes read do not match");
			} while (master.Position < master.Length);
		}

		[Test]
		public async void TestReadAsync ()
		{
			blocks.Position = 0;
			master.Position = 0;

			do {
				int nread = await blocks.ReadAsync (buf, 0, buf.Length);
				int mread = await master.ReadAsync (mbuf, 0, mbuf.Length);

				Assert.AreEqual (mread, nread, "Did not read the expected number of bytes from the memory block stream");
				Assert.AreEqual (master.Position, blocks.Position, "The memory block stream's position did not match");

				for (int i = 0; i < mread; i++)
					Assert.AreEqual (mbuf[i], buf[i], "The bytes read do not match");
			} while (master.Position < master.Length);
		}

		[Test]
		public void TestWrite ()
		{
			var bytes = new byte[9 * 1024];
			int position = 0;

			random.NextBytes (bytes);

			blocks.Position = 0;
			master.Position = 0;

			while (position < bytes.Length) {
				int n = Math.Min (bytes.Length - position, random.Next () % 4096);
				blocks.Write (bytes, position, n);
				master.Write (bytes, position, n);
				position += n;
			}

			blocks.Flush ();
			master.Flush ();
		}

		[Test]
		public async void TestWriteAsync ()
		{
			var bytes = new byte[9 * 1024];
			int position = 0;

			random.NextBytes (bytes);

			blocks.Position = 0;
			master.Position = 0;

			while (position < bytes.Length) {
				int n = Math.Min (bytes.Length - position, random.Next () % 4096);
				await blocks.WriteAsync (bytes, position, n);
				await master.WriteAsync (bytes, position, n);
				position += n;
			}

			await blocks.FlushAsync ();
			await master.FlushAsync ();
		}

		void AssertSeekResults ()
		{
			int n = (int) Math.Min (master.Length - master.Position, mbuf.Length);
			int mread = master.Read (mbuf, 0, n);
			int nread = blocks.Read (buf, 0, n);

			Assert.AreEqual (mread, nread, "Did not read the expected number of bytes from the memory block stream");
			Assert.AreEqual (master.Position, blocks.Position, "The memory block stream's position did not match");

			for (int i = 0; i < n; i++)
				Assert.AreEqual (mbuf[i], buf[i], "The bytes read do not match");
		}

		[Test]
		public void TestSeek ()
		{
			for (int attempt = 0; attempt < 10; attempt++) {
				long offset = random.Next () % master.Length;

				long expected = master.Seek (offset, SeekOrigin.Begin);
				long actual = blocks.Seek (offset, SeekOrigin.Begin);

				Assert.AreEqual (expected, actual, "SeekOrigin.Begin");

				AssertSeekResults ();
				master.Seek (actual, SeekOrigin.Begin);
				blocks.Seek (actual, SeekOrigin.Begin);

				// seek backwards from current position
				offset = -1 * (random.Next () % offset);

				expected = master.Seek (offset, SeekOrigin.Current);
				actual = blocks.Seek (offset, SeekOrigin.Current);

				Assert.AreEqual (expected, actual, "SeekOrigin.Current (-)");

				AssertSeekResults ();
				master.Seek (actual, SeekOrigin.Begin);
				blocks.Seek (actual, SeekOrigin.Begin);

				// seek forwards from current position
				offset = random.Next () % (master.Length - actual);

				expected = master.Seek (offset, SeekOrigin.Current);
				actual = blocks.Seek (offset, SeekOrigin.Current);

				Assert.AreEqual (expected, actual, "SeekOrigin.Current (+)");

				AssertSeekResults ();

				// seek backwards from the end of the stream
				offset = -1 * (random.Next () % master.Length);

				expected = master.Seek (offset, SeekOrigin.End);
				actual = blocks.Seek (offset, SeekOrigin.End);

				Assert.AreEqual (expected, actual, "SeekOrigin.End");

				AssertSeekResults ();
			}

			Assert.Throws<IOException> (() => blocks.Seek (-1, SeekOrigin.Begin));
		}

		[Test]
		public void TestSetLength ()
		{
			long length = blocks.Length;

			Assert.Throws<ArgumentOutOfRangeException> (() => blocks.SetLength (-1));

			blocks.SetLength (length + 10240);

			Assert.AreEqual (length + 10240, blocks.Length);

			blocks.SetLength (length);

			Assert.AreEqual (length, blocks.Length);
		}

		[Test]
		public void TestToArray ()
		{
			var masterArray = master.ToArray ();
			var array = blocks.ToArray ();

			Assert.AreEqual (masterArray.Length, array.Length, "ToArray() length does not match");

			for (int i = 0; i < array.Length; i++)
				Assert.AreEqual (masterArray[i], array[i], "The bytes do not match");
		}
	}
}
