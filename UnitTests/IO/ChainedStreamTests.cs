//
// ChainedStreamTests.cs
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
using MimeKit.IO;

namespace UnitTests.IO {
	[TestFixture]
	public class ChainedStreamTests : IDisposable
	{
		readonly List<int> lengths = new List<int> ();
		readonly MemoryStream master, backing;
		readonly ChainedStream chained;
		readonly byte[] cbuf, mbuf;
		readonly Random random;

		public ChainedStreamTests ()
		{
			var bytes = new byte[10 * 1024];
			int position = 0;

			random = new Random ();
			random.NextBytes (bytes);

			// this is our master stream, all operations on the chained stream
			// should match the results on this stream
			master = new MemoryStream (bytes);
			backing = new MemoryStream (master.ToArray ());
			cbuf = new byte[4096];
			mbuf = new byte[4096];

			// make a handful of smaller streams based on master to chain together
			chained = new ChainedStream ();
			while (position < bytes.Length) {
				int n = Math.Min (bytes.Length - position, random.Next () % 4096);

				var stream = new BoundStream (backing, position, position + n, true);

				lengths.Add (n);
				position += n;

				chained.Add (new ReadOneByteStream (stream));
			}
		}

		public void Dispose ()
		{
			backing.Dispose ();
			chained.Dispose ();
			master.Dispose ();
		}

		[Test]
		public void TestCanReadWriteSeek ()
		{
			var buffer = new byte[1024];

			using (var chained = new ChainedStream ()) {
				chained.Add (new CanReadWriteSeekStream (true, false, false, false));

				Assert.That (chained.CanRead, Is.True);
				Assert.That (chained.CanWrite, Is.False);
				Assert.That (chained.CanSeek, Is.False);
				Assert.That (chained.CanTimeout, Is.False);

				Assert.Throws<NotImplementedException> (() => chained.Read (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => chained.Write (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => chained.Seek (0, SeekOrigin.End));
			}

			using (var chained = new ChainedStream ()) {
				chained.Add (new CanReadWriteSeekStream (false, true, false, false));

				Assert.That (chained.CanRead, Is.False);
				Assert.That (chained.CanWrite, Is.True);
				Assert.That (chained.CanSeek, Is.False);
				Assert.That (chained.CanTimeout, Is.False);

				Assert.Throws<NotSupportedException> (() => chained.Read (buffer, 0, buffer.Length));
				Assert.Throws<NotImplementedException> (() => chained.Write (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => chained.Seek (0, SeekOrigin.End));
			}

			using (var chained = new ChainedStream ()) {
				chained.Add (new CanReadWriteSeekStream (false, false, true, false));

				Assert.That (chained.CanRead, Is.False);
				Assert.That (chained.CanWrite, Is.False);
				Assert.That (chained.CanSeek, Is.True);
				Assert.That (chained.CanTimeout, Is.False);

				Assert.Throws<NotSupportedException> (() => chained.Read (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => chained.Write (buffer, 0, buffer.Length));
				Assert.Throws<NotImplementedException> (() => chained.Seek (0, SeekOrigin.End));
			}
		}

		[Test]
		public void TestGetSetTimeouts ()
		{
			using (var chained = new ChainedStream ()) {
				chained.Add (new TimeoutStream ());

				Assert.Throws<InvalidOperationException> (() => { int x = chained.ReadTimeout; });
				Assert.Throws<InvalidOperationException> (() => { int x = chained.WriteTimeout; });

				Assert.Throws<InvalidOperationException> (() => chained.ReadTimeout = 5);
				Assert.Throws<InvalidOperationException> (() => chained.WriteTimeout = 5);
			}
		}

		[Test]
		public void TestRead ()
		{
			Assert.That (chained.CanRead, Is.True, "Expected to be able to read from the chained stream.");

			Assert.That (chained.Read (cbuf, 0, 0), Is.EqualTo (0), "Zero-length read");

			do {
				int n = (int) Math.Min (master.Length - master.Position, mbuf.Length);
				int nread = chained.Read (cbuf, 0, n);
				int mread = master.Read (mbuf, 0, n);

				Assert.That (nread, Is.EqualTo (mread), "Did not read the expected number of bytes from the chained stream");
				Assert.That (chained.Position, Is.EqualTo (master.Position), "The chained stream's position did not match");

				for (int i = 0; i < n; i++)
					Assert.That (cbuf[i], Is.EqualTo (mbuf[i]), "The bytes read do not match");
			} while (master.Position < master.Length);
		}

		[Test]
		public async Task TestReadAsync ()
		{
			Assert.That (chained.CanRead, Is.True, "Expected to be able to read from the chained stream.");

			Assert.That (await chained.ReadAsync (cbuf, 0, 0), Is.EqualTo (0), "Zero-length read");

			do {
				int n = (int) Math.Min (master.Length - master.Position, mbuf.Length);
				int nread = await chained.ReadAsync (cbuf, 0, n);
				int mread = await master.ReadAsync (mbuf, 0, n);

				Assert.That (nread, Is.EqualTo (mread), "Did not read the expected number of bytes from the chained stream");
				Assert.That (chained.Position, Is.EqualTo (master.Position), "The chained stream's position did not match");

				for (int i = 0; i < n; i++)
					Assert.That (cbuf[i], Is.EqualTo (mbuf[i]), "The bytes read do not match");
			} while (master.Position < master.Length);
		}

		void AssertSeekResults (string operation)
		{
			int n = (int) Math.Min (master.Length - master.Position, 256);
			int nread = chained.Read (cbuf, 0, n);
			int mread = master.Read (mbuf, 0, n);

			Assert.That (nread, Is.EqualTo (mread), $"Did not read the expected number of bytes from the chained stream after {operation}");
			Assert.That (chained.Position, Is.EqualTo (master.Position), $"The chained stream's position did not match after {operation}");

			for (int i = 0; i < n; i++)
				Assert.That (cbuf[i], Is.EqualTo (mbuf[i]), $"The bytes read do not match after {operation}");
		}

		[Test]
		public void TestRandomSeeking ()
		{
			Assert.That (chained.CanSeek, Is.True, "Expected to be able to seek in the chained stream.");

			Assert.Throws<IOException> (() => chained.Seek (-1, SeekOrigin.Begin));
			Assert.Throws<IOException> (() => chained.Seek (int.MaxValue, SeekOrigin.Begin));

			for (int attempt = 0; attempt < 10; attempt++) {
				long offset = random.Next () % master.Length;
				var origin = (SeekOrigin) (attempt % 3);
				long expected, actual;

				switch (origin) {
				case SeekOrigin.Current: offset -= master.Position; break;
				case SeekOrigin.End: offset -= master.Length; break;
				}

				if (origin == SeekOrigin.Begin) {
					chained.Position = offset;
					master.Position = offset;

					expected = master.Position;
					actual = chained.Position;
				} else {
					expected = master.Seek (offset, origin);
					actual = chained.Seek (offset, origin);
				}

				Assert.That (actual, Is.EqualTo (expected), "Seeking the chained stream did not return the expected position");

				AssertSeekResults ("seeking to random position");
			}
		}

		[Test]
		public void TestSeekingToCurrentPosition ()
		{
			long expected, actual;

			expected = master.Seek (99, SeekOrigin.Begin);
			actual = chained.Seek (99, SeekOrigin.Begin);

			Assert.That (actual, Is.EqualTo (expected), "Seeking the chained stream did not return the expected position");

			AssertSeekResults ("seeking to offset 99");

			expected = master.Seek (master.Position, SeekOrigin.Begin);
			actual = chained.Seek (chained.Position, SeekOrigin.Begin);

			Assert.That (actual, Is.EqualTo (expected), "Seeking the chained stream did not return the expected position");

			AssertSeekResults ("seeking to current position via SeekOrigin.Begin");

			expected = master.Seek (0, SeekOrigin.Current);
			actual = chained.Seek (0, SeekOrigin.Current);

			Assert.That (actual, Is.EqualTo (expected), "Seeking the chained stream did not return the expected position");

			AssertSeekResults ("seeking to current position via SeekOrigin.Current");

			expected = master.Seek (master.Position - master.Length, SeekOrigin.End);
			actual = chained.Seek (chained.Position - chained.Length, SeekOrigin.End);

			Assert.That (actual, Is.EqualTo (expected), "Seeking the chained stream did not return the expected position");

			AssertSeekResults ("seeking to current position via SeekOrigin.End");
		}

		[Test]
		public void TestSeekingToStreamBoundaries ()
		{
			long expected, actual;

			// first, seek to the beginning
			expected = master.Seek (0, SeekOrigin.Begin);
			actual = chained.Seek (0, SeekOrigin.Begin);

			Assert.That (actual, Is.EqualTo (expected), "Seeking the chained stream did not return the expected position");

			AssertSeekResults ("seeking to the beginning");

			// now seek to the second boundary
			expected = master.Seek (lengths[1], SeekOrigin.Begin);
			actual = chained.Seek (lengths[1], SeekOrigin.Begin);

			Assert.That (actual, Is.EqualTo (expected), "Seeking the chained stream did not return the expected position");

			AssertSeekResults ("seeking to the second boundary");

			// now seek to the first boundary
			expected = master.Seek (lengths[0], SeekOrigin.Begin);
			actual = chained.Seek (lengths[0], SeekOrigin.Begin);

			Assert.That (actual, Is.EqualTo (expected), "Seeking the chained stream did not return the expected position");

			AssertSeekResults ("seeking to the first boundary");
		}

		async Task AssertSeekResultsAsync (string operation)
		{
			int n = (int) Math.Min (master.Length - master.Position, 256);
			int nread = await chained.ReadAsync (cbuf, 0, n);
			int mread = await master.ReadAsync (mbuf, 0, n);

			Assert.That (nread, Is.EqualTo (mread), $"Did not read the expected number of bytes from the chained stream after {operation}");
			Assert.That (chained.Position, Is.EqualTo (master.Position), $"The chained stream's position did not match after {operation}");

			for (int i = 0; i < n; i++)
				Assert.That (cbuf[i], Is.EqualTo (mbuf[i]), $"The bytes read do not match after {operation}");
		}

		[Test]
		public async Task TestRandomSeekingAsync ()
		{
			Assert.That (chained.CanSeek, Is.True, "Expected to be able to seek in the chained stream.");

			Assert.Throws<IOException> (() => chained.Seek (-1, SeekOrigin.Begin));
			Assert.Throws<IOException> (() => chained.Seek (int.MaxValue, SeekOrigin.Begin));

			for (int attempt = 0; attempt < 10; attempt++) {
				long offset = random.Next () % master.Length;
				var origin = (SeekOrigin) (attempt % 3);
				long expected, actual;

				switch (origin) {
				case SeekOrigin.Current: offset -= master.Position; break;
				case SeekOrigin.End: offset -= master.Length; break;
				}

				if (origin == SeekOrigin.Begin) {
					chained.Position = offset;
					master.Position = offset;

					expected = master.Position;
					actual = chained.Position;
				} else {
					expected = master.Seek (offset, origin);
					actual = chained.Seek (offset, origin);
				}

				Assert.That (actual, Is.EqualTo (expected), "Seeking the chained stream did not return the expected position");

				await AssertSeekResultsAsync ("seeking to random position");
			}
		}

		[Test]
		public async Task TestSeekingToStreamBoundariesAsync ()
		{
			long expected, actual;

			// first, seek to the beginning
			expected = master.Seek (0, SeekOrigin.Begin);
			actual = chained.Seek (0, SeekOrigin.Begin);

			Assert.That (actual, Is.EqualTo (expected), "Seeking the chained stream did not return the expected position");

			AssertSeekResults ("seeking to the beginning");

			// now seek to the second boundary
			expected = master.Seek (lengths[1], SeekOrigin.Begin);
			actual = chained.Seek (lengths[1], SeekOrigin.Begin);

			Assert.That (actual, Is.EqualTo (expected), "Seeking the chained stream did not return the expected position");

			AssertSeekResults ("seeking to the second boundary");

			// now seek to the first boundary
			expected = master.Seek (lengths[0], SeekOrigin.Begin);
			actual = chained.Seek (lengths[0], SeekOrigin.Begin);

			Assert.That (actual, Is.EqualTo (expected), "Seeking the chained stream did not return the expected position");

			await AssertSeekResultsAsync ("seeking to the first boundary");
		}

		[Test]
		public void TestWrite ()
		{
			var buffer = new byte[(int) chained.Length];

			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = (byte) (i & 0xff);

			chained.Position = 0;
			chained.Write (buffer, 0, buffer.Length);
			chained.Flush ();

			var array = backing.ToArray ();
			for (int i = 0; i < buffer.Length; i++)
				Assert.That (array[i], Is.EqualTo (buffer[i]), $"Written byte @ offset {i} did not match");
		}

		[Test]
		public async Task TestWriteAsync ()
		{
			var buffer = new byte[(int) chained.Length];

			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = (byte) (i & 0xff);

			chained.Position = 0;
			await chained.WriteAsync (buffer, 0, buffer.Length);
			await chained.FlushAsync ();

			var array = backing.ToArray ();
			for (int i = 0; i < buffer.Length; i++)
				Assert.That (array[i], Is.EqualTo (buffer[i]), $"Written byte @ offset {i} did not match");
		}

		[Test]
		public void TestWriteOverflow ()
		{
			var buffer = new byte[(int) chained.Length + 1];

			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = (byte) (i & 0xff);

			chained.Position = 0;
			Assert.Throws<IOException> (() => chained.Write (buffer, 0, buffer.Length));

			chained.Position = 0;
			chained.Write (buffer, 0, buffer.Length - 1);
			Assert.Throws<IOException> (() => chained.Write (buffer, buffer.Length - 1, 1));
		}

		[Test]
		public async Task TestWriteOverflowAsync ()
		{
			var buffer = new byte[(int) chained.Length + 1];

			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = (byte) (i & 0xff);

			chained.Position = 0;
			Assert.ThrowsAsync<IOException> (async () => await chained.WriteAsync (buffer, 0, buffer.Length));

			chained.Position = 0;
			await chained.WriteAsync (buffer, 0, buffer.Length - 1);
			Assert.ThrowsAsync<IOException> (async () => await chained.WriteAsync (buffer, buffer.Length - 1, 1));
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

			using (var chained = new ChainedStream ()) {
				chained.Add (headers);
				chained.Add (content);

				var entity = MimeEntity.Load (chained, true) as TextPart;

				Assert.That (entity.Text, Is.EqualTo ("Hello, world!" + Environment.NewLine));
			}
		}

		[Test]
		public void TestSetLength ()
		{
			Assert.Throws<NotSupportedException> (() => chained.SetLength (500));
		}
	}
}
