//
// MemoryBlockStreamTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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

namespace UnitTests {
	[TestFixture]
	public class MemoryBlockStreamTests
	{
		MemoryBlockStream blocks;
		MemoryStream master;
		byte[] buf, mbuf;
		Random random;

		[TestFixtureSetUp]
		public void Setup ()
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

			Assert.IsTrue (blocks.CanRead, "Expected to be able to read from the memory block stream.");
			Assert.IsTrue (blocks.CanWrite, "Expected to be able to write to the memory block stream.");
			Assert.IsTrue (blocks.CanSeek, "Expected to be able to seek in the memory block stream.");
			Assert.IsFalse (blocks.CanTimeout, "Did not expect to be able to set timeouts in the memory block stream.");

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
		public void TestReading ()
		{
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
		public void TestSeeking ()
		{
			for (int attempt = 0; attempt < 10; attempt++) {
				long offset = random.Next () % master.Length;

				long expected = master.Seek (offset, SeekOrigin.Begin);
				long actual = blocks.Seek (offset, SeekOrigin.Begin);

				Assert.AreEqual (expected, actual, "Seeking the memory block stream did not return the expected position");

				int n = (int) Math.Min (master.Length - master.Position, mbuf.Length);
				int mread = master.Read (mbuf, 0, n);
				int nread = blocks.Read (buf, 0, n);

				Assert.AreEqual (mread, nread, "Did not read the expected number of bytes from the memory block stream");
				Assert.AreEqual (master.Position, blocks.Position, "The memory block stream's position did not match");

				for (int i = 0; i < n; i++)
					Assert.AreEqual (mbuf[i], buf[i], "The bytes read do not match");
			}
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
