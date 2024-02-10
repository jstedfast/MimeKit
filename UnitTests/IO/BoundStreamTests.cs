//
// BoundStreamTests.cs
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

using MimeKit.IO;

namespace UnitTests.IO {
	[TestFixture]
	public class BoundStreamTests
	{
		[Test]
		public void TestCanReadWriteSeek ()
		{
			var buffer = new byte[1024];

			using (var bounded = new BoundStream (new CanReadWriteSeekStream (true, false, false, false), 0, -1, false)) {
				Assert.That (bounded.CanRead, Is.True);
				Assert.That (bounded.CanWrite, Is.False);
				Assert.That (bounded.CanSeek, Is.False);
				Assert.That (bounded.CanTimeout, Is.False);

				Assert.Throws<NotImplementedException> (() => bounded.Read (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => bounded.Write (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => bounded.Seek (0, SeekOrigin.End));
			}

			using (var bounded = new BoundStream (new CanReadWriteSeekStream (false, true, false, false), 0, -1, false)) {
				Assert.That (bounded.CanRead, Is.False);
				Assert.That (bounded.CanWrite, Is.True);
				Assert.That (bounded.CanSeek, Is.False);
				Assert.That (bounded.CanTimeout, Is.False);

				Assert.Throws<NotSupportedException> (() => bounded.Read (buffer, 0, buffer.Length));
				Assert.Throws<NotImplementedException> (() => bounded.Write (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => bounded.Seek (0, SeekOrigin.End));
			}

			using (var bounded = new BoundStream (new CanReadWriteSeekStream (false, false, true, false), 0, -1, false)) {
				Assert.That (bounded.CanRead, Is.False);
				Assert.That (bounded.CanWrite, Is.False);
				Assert.That (bounded.CanSeek, Is.True);
				Assert.That (bounded.CanTimeout, Is.False);

				Assert.Throws<NotSupportedException> (() => bounded.Read (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => bounded.Write (buffer, 0, buffer.Length));
				Assert.Throws<NotImplementedException> (() => bounded.Seek (0, SeekOrigin.End));
			}
		}

		[Test]
		public void TestGetSetTimeouts ()
		{
			using (var bounded = new BoundStream (new TimeoutStream (), 0, -1, false)) {
				Assert.That (bounded.ReadTimeout, Is.EqualTo (0));
				Assert.That (bounded.WriteTimeout, Is.EqualTo (0));

				bounded.ReadTimeout = 10;
				Assert.That (bounded.ReadTimeout, Is.EqualTo (10));

				bounded.WriteTimeout = 100;
				Assert.That (bounded.WriteTimeout, Is.EqualTo (100));
			}
		}

		[Test]
		public void TestSeek ()
		{
			using (var memory = new MemoryStream ()) {
				var buffer = Encoding.ASCII.GetBytes ("This is some text...");

				memory.Write (buffer, 0, buffer.Length);

				using (var bounded = new BoundStream (memory, 5, -1, true)) {
					long position;
					string text;
					int n;

					// make sure that BoundStream will properly reset the underlying stream
					Assert.That (bounded.Position, Is.EqualTo (0));
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.That (text, Is.EqualTo ("is some text..."));

					// force eos state to be true
					bounded.Read (buffer, 0, buffer.Length);

					position = bounded.Seek (-1 * n, SeekOrigin.End);
					Assert.That (position, Is.EqualTo (0), "SeekOrigin.End");
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.That (text, Is.EqualTo ("is some text..."));

					position = bounded.Seek (0, SeekOrigin.Begin);
					Assert.That (position, Is.EqualTo (0), "SeekOrigin.Begin");
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.That (text, Is.EqualTo ("is some text..."));

					position = bounded.Seek (-1 * n, SeekOrigin.Current);
					Assert.That (position, Is.EqualTo (0), "SeekOrigin.Current");
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.That (text, Is.EqualTo ("is some text..."));

					// now try seeking out of bounds
					Assert.Throws<IOException> (() => bounded.Seek (-1, SeekOrigin.Begin));
					Assert.Throws<IOException> (() => bounded.Seek (-1 * buffer.Length, SeekOrigin.End));
				}
			}

			using (var memory = new MemoryStream ()) {
				var buffer = Encoding.ASCII.GetBytes ("This is some text...");

				memory.Write (buffer, 0, buffer.Length);

				using (var bounded = new BoundStream (memory, 5, buffer.Length, true)) {
					long position;
					string text;
					int n;

					// make sure that BoundStream will properly reset the underlying stream
					Assert.That (bounded.Position, Is.EqualTo (0));
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.That (text, Is.EqualTo ("is some text..."));

					position = bounded.Seek (0, SeekOrigin.Begin);
					Assert.That (position, Is.EqualTo (0), "SeekOrigin.Begin");
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.That (text, Is.EqualTo ("is some text..."));

					position = bounded.Seek (-1 * n, SeekOrigin.Current);
					Assert.That (position, Is.EqualTo (0), "SeekOrigin.Current");
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.That (text, Is.EqualTo ("is some text..."));

					position = bounded.Seek (-1 * n, SeekOrigin.End);
					Assert.That (position, Is.EqualTo (0), "SeekOrigin.End");
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.That (text, Is.EqualTo ("is some text..."));

					// now try seeking out of bounds
					Assert.Throws<IOException> (() => bounded.Seek (-1, SeekOrigin.Begin));
					Assert.Throws<IOException> (() => bounded.Seek (-1 * buffer.Length, SeekOrigin.End));
					Assert.Throws<IOException> (() => bounded.Seek (5, SeekOrigin.End));
				}
			}
		}

		[Test]
		public void TestSetLength ()
		{
			using (var memory = new MemoryStream ()) {
				var buffer = Encoding.ASCII.GetBytes ("This is some text...");

				memory.Write (buffer, 0, buffer.Length);

				using (var bounded = new BoundStream (memory, 0, -1, true)) {
					var buf = new byte[1024];

					Assert.That (bounded.Length, Is.EqualTo (buffer.Length));

					bounded.Read (buf, 0, buf.Length); // read the text
					bounded.Read (buf, 0, buf.Length); // cause eos to be true

					Assert.That (bounded.Length, Is.EqualTo (buffer.Length));

					bounded.SetLength (500);

					Assert.That (bounded.Length, Is.EqualTo (500));
					Assert.That (memory.Length, Is.EqualTo (500));
				}
			}

			using (var memory = new MemoryStream ()) {
				var buffer = Encoding.ASCII.GetBytes ("This is some text...");

				memory.Write (buffer, 0, buffer.Length);

				using (var bounded = new BoundStream (memory, 0, buffer.Length, true)) {
					Assert.That (bounded.Length, Is.EqualTo (buffer.Length));

					bounded.SetLength (500);

					Assert.That (bounded.Length, Is.EqualTo (500));
					Assert.That (memory.Length, Is.EqualTo (500));
				}
			}

			using (var memory = new MemoryStream ()) {
				var buffer = Encoding.ASCII.GetBytes ("This is some text...");

				memory.Write (buffer, 0, buffer.Length);

				using (var bounded = new BoundStream (memory, 0, buffer.Length, true)) {
					Assert.That (bounded.Length, Is.EqualTo (buffer.Length));

					bounded.SetLength (5);

					Assert.That (bounded.Length, Is.EqualTo (5));
					Assert.That (memory.Length, Is.EqualTo (buffer.Length));
				}
			}
		}

		[Test]
		public void TestSetPosition ()
		{
			using (var memory = new MemoryStream ()) {
				var buffer = Encoding.ASCII.GetBytes ("This is some text...");

				memory.Write (buffer, 0, buffer.Length);
				memory.Position = 0;

				using (var bounded = new BoundStream (memory, 0, -1, true)) {
					bounded.Position = 10;

					Assert.That (bounded.Position, Is.EqualTo (10), "BoundedStream position");
					Assert.That (memory.Position, Is.EqualTo (10), "MemoryStream position");
				}
			}
		}

		[Test]
		public void TestWritingBeyondEndBoundary ()
		{
			using (var memory = new MemoryStream ()) {
				var buffer = new byte[] { (byte) 'A' };

				memory.Write (buffer, 0, buffer.Length);
				memory.Position = 0;

				using (var bounded = new BoundStream (memory, 0, 2, true)) {
					buffer = new byte[] { (byte) 'b', (byte) 'c', (byte) 'd' };

					Assert.Throws<IOException> (() => bounded.Write (buffer, 0, buffer.Length));
				}
			}
		}

		[Test]
		public void TestWritingBeyondEndBoundaryAsync ()
		{
			using (var memory = new MemoryStream ()) {
				var buffer = new byte[] { (byte) 'A' };

				memory.Write (buffer, 0, buffer.Length);
				memory.Position = 0;

				using (var bounded = new BoundStream (memory, 0, 2, true)) {
					buffer = new byte[] { (byte) 'b', (byte) 'c', (byte) 'd' };

					Assert.ThrowsAsync<IOException> (async () => await bounded.WriteAsync (buffer, 0, buffer.Length));
				}
			}
		}
	}
}
