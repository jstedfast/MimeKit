//
// ChainedStreamTests.cs
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
using System.Text;
using System.Collections.Generic;

using NUnit.Framework;

using MimeKit.IO;
using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class ChainedStreamTests
	{
		readonly List<int> lengths = new List<int> ();
		ChainedStream chained;
		MemoryStream master;
		byte[] cbuf, mbuf;
		Random random;

		[TestFixtureSetUp]
		public void Setup ()
		{
			var bytes = new byte[10 * 1024];
			int position = 0;

			random = new Random ();
			random.NextBytes (bytes);

			// this is our master stream, all operations on the chained stream
			// should match the results on this stream
			master = new MemoryStream (bytes);
			cbuf = new byte[4096];
			mbuf = new byte[4096];

			// make a handful of smaller streams based on master to chain together
			chained = new ChainedStream ();
			while (position < bytes.Length) {
				int n = Math.Min (bytes.Length - position, random.Next () % 4096);

				var segment = new byte[n];
				Buffer.BlockCopy (bytes, position, segment, 0, n);
				lengths.Add (n);
				position += n;

				chained.Add (new ReadOneByteStream (new MemoryStream (segment)));
			}
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			chained.Dispose ();
			master.Dispose ();
		}

		[Test]
		public void TestReading ()
		{
			Assert.IsTrue (chained.CanRead, "Expected to be able to read from the chained stream.");

			do {
				int n = (int) Math.Min (master.Length - master.Position, mbuf.Length);
				int nread = chained.Read (cbuf, 0, n);
				int mread = master.Read (mbuf, 0, n);

				Assert.AreEqual (mread, nread, "Did not read the expected number of bytes from the chained stream");
				Assert.AreEqual (master.Position, chained.Position, "The chained stream's position did not match");

				for (int i = 0; i < n; i++)
					Assert.AreEqual (mbuf[i], cbuf[i], "The bytes read do not match");
			} while (master.Position < master.Length);
		}

		void AssertSeekResults (string operation)
		{
			int n = (int) Math.Min (master.Length - master.Position, mbuf.Length);
			int nread = chained.Read (cbuf, 0, n);
			int mread = master.Read (mbuf, 0, n);

			Assert.AreEqual (mread, nread, "Did not read the expected number of bytes from the chained stream after {0}", operation);
			Assert.AreEqual (master.Position, chained.Position, "The chained stream's position did not match after {0}", operation);

			for (int i = 0; i < n; i++)
				Assert.AreEqual (mbuf[i], cbuf[i], "The bytes read do not match after {0}", operation);
		}

		[Test]
		public void TestRandomSeeking ()
		{
			Assert.IsTrue (chained.CanSeek, "Expected to be able to seek in the chained stream.");

			for (int attempt = 0; attempt < 10; attempt++) {
				long offset = random.Next () % master.Length;

				long expected = master.Seek (offset, SeekOrigin.Begin);
				long actual = chained.Seek (offset, SeekOrigin.Begin);

				Assert.AreEqual (expected, actual, "Seeking the chained stream did not return the expected position");

				AssertSeekResults ("seeking to random position");
			}
		}

		[Test]
		public void TestSeekingToStreamBoundaries ()
		{
			long expected, actual;

			// first, seek to the beginning
			expected = master.Seek (0, SeekOrigin.Begin);
			actual = chained.Seek (0, SeekOrigin.Begin);

			Assert.AreEqual (expected, actual, "Seeking the chained stream did not return the expected position");

			AssertSeekResults ("seeking to the beginning");

			// now seek to the second boundary
			expected = master.Seek (lengths[1], SeekOrigin.Begin);
			actual = chained.Seek (lengths[1], SeekOrigin.Begin);

			Assert.AreEqual (expected, actual, "Seeking the chained stream did not return the expected position");

			AssertSeekResults ("seeking to the second boundary");

			// now seek to the first boundary
			expected = master.Seek (lengths[0], SeekOrigin.Begin);
			actual = chained.Seek (lengths[0], SeekOrigin.Begin);

			Assert.AreEqual (expected, actual, "Seeking the chained stream did not return the expected position");

			AssertSeekResults ("seeking to the first boundary");
		}

		[Test]
		public void TestChainedHeadersAndContent ()
		{
			var buf = Encoding.ASCII.GetBytes ("Content-Type: text/plain\r\n\r\n");
			var headers = new MemoryStream ();
			var content = new MemoryStream ();

			headers.Write (buf, 0, buf.Length);
			headers.Position = 0;

			buf = Encoding.ASCII.GetBytes ("Hello, world!\r\n");

			content.Write (buf, 0, buf.Length);
			content.Position = 0;

			var chained = new ChainedStream ();
			chained.Add (headers);
			chained.Add (content);

			var entity = MimeEntity.Load (chained, true) as TextPart;

			Assert.AreEqual ("Hello, world!\r\n", entity.Text);
		}
	}

	class ReadOneByteStream : Stream
	{
		readonly Stream source;

		public ReadOneByteStream (Stream source)
		{
			this.source = source;
		}

		public override bool CanRead {
			get { return source.CanRead; }
		}

		public override bool CanWrite {
			get { return source.CanWrite; }
		}

		public override bool CanSeek {
			get { return source.CanSeek; }
		}

		public override long Length {
			get { return source.Length; }
		}

		public override long Position {
			get { return source.Position; }
			set { source.Position = value; }
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return source.Read (buffer, offset, 1);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			source.Write (buffer, offset, count);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			return source.Seek (offset, origin);
		}

		public override void Flush ()
		{
			source.Flush ();
		}

		public override void SetLength (long value)
		{
			source.SetLength (value);
		}
	}
}
