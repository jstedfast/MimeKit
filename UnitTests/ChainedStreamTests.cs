//
// ChainedStreamTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class ChainedStreamTests
	{
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

			// make a handful of smaller streams absed on master to chain together
			chained = new ChainedStream ();
			while (position < bytes.Length) {
				int n = Math.Min (bytes.Length - position, random.Next () % 4096);

				var segment = new byte[n];
				Array.Copy (bytes, position, segment, 0, n);
				position += n;

				chained.Add (new MemoryStream (segment));
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

		[Test]
		public void TestSeeking ()
		{
			for (int attempt = 0; attempt < 10; attempt++) {
				long offset = random.Next () % master.Length;

				long expected = master.Seek (offset, SeekOrigin.Begin);
				long actual = chained.Seek (offset, SeekOrigin.Begin);

				Assert.AreEqual (expected, actual, "Seeking the chained stream did not return the expected position");

				int n = (int) Math.Min (master.Length - master.Position, mbuf.Length);
				int nread = chained.Read (cbuf, 0, n);
				int mread = master.Read (mbuf, 0, n);

				Assert.AreEqual (mread, nread, "Did not read the expected number of bytes from the chained stream");
				Assert.AreEqual (master.Position, chained.Position, "The chained stream's position did not match");

				for (int i = 0; i < n; i++)
					Assert.AreEqual (mbuf[i], cbuf[i], "The bytes read do not match");
			}
		}
	}
}
