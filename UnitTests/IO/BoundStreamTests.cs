//
// BoundStreamTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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

using NUnit.Framework;

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
				Assert.IsTrue (bounded.CanRead);
				Assert.IsFalse (bounded.CanWrite);
				Assert.IsFalse (bounded.CanSeek);
				Assert.IsFalse (bounded.CanTimeout);

				Assert.Throws<NotImplementedException> (() => bounded.Read (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => bounded.Write (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => bounded.Seek (0, SeekOrigin.End));
			}

			using (var bounded = new BoundStream (new CanReadWriteSeekStream (false, true, false, false), 0, -1, false)) {
				Assert.IsFalse (bounded.CanRead);
				Assert.IsTrue (bounded.CanWrite);
				Assert.IsFalse (bounded.CanSeek);
				Assert.IsFalse (bounded.CanTimeout);

				Assert.Throws<NotSupportedException> (() => bounded.Read (buffer, 0, buffer.Length));
				Assert.Throws<NotImplementedException> (() => bounded.Write (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => bounded.Seek (0, SeekOrigin.End));
			}

			using (var bounded = new BoundStream (new CanReadWriteSeekStream (false, false, true, false), 0, -1, false)) {
				Assert.IsFalse (bounded.CanRead);
				Assert.IsFalse (bounded.CanWrite);
				Assert.IsTrue (bounded.CanSeek);
				Assert.IsFalse (bounded.CanTimeout);

				Assert.Throws<NotSupportedException> (() => bounded.Read (buffer, 0, buffer.Length));
				Assert.Throws<NotSupportedException> (() => bounded.Write (buffer, 0, buffer.Length));
				Assert.Throws<NotImplementedException> (() => bounded.Seek (0, SeekOrigin.End));
			}
		}

		[Test]
		public void TestGetSetTimeouts ()
		{
			using (var bounded = new BoundStream (new TimeoutStream (), 0, -1, false)) {
				Assert.AreEqual (0, bounded.ReadTimeout);
				Assert.AreEqual (0, bounded.WriteTimeout);

				bounded.ReadTimeout = 10;
				Assert.AreEqual (10, bounded.ReadTimeout);

				bounded.WriteTimeout = 100;
				Assert.AreEqual (100, bounded.WriteTimeout);
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
					Assert.AreEqual (0, bounded.Position);
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.AreEqual ("is some text...", text);

					// force eos state to be true
					bounded.Read (buffer, 0, buffer.Length);

					position = bounded.Seek (-1 * n, SeekOrigin.End);
					Assert.AreEqual (0, position, "SeekOrigin.End");
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.AreEqual ("is some text...", text);

					position = bounded.Seek (0, SeekOrigin.Begin);
					Assert.AreEqual (0, position, "SeekOrigin.Begin");
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.AreEqual ("is some text...", text);

					position = bounded.Seek (-1 * n, SeekOrigin.Current);
					Assert.AreEqual (0, position, "SeekOrigin.Current");
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.AreEqual ("is some text...", text);

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
					Assert.AreEqual (0, bounded.Position);
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.AreEqual ("is some text...", text);

					position = bounded.Seek (0, SeekOrigin.Begin);
					Assert.AreEqual (0, position, "SeekOrigin.Begin");
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.AreEqual ("is some text...", text);

					position = bounded.Seek (-1 * n, SeekOrigin.Current);
					Assert.AreEqual (0, position, "SeekOrigin.Current");
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.AreEqual ("is some text...", text);

					position = bounded.Seek (-1 * n, SeekOrigin.End);
					Assert.AreEqual (0, position, "SeekOrigin.End");
					n = bounded.Read (buffer, 0, buffer.Length);
					text = Encoding.ASCII.GetString (buffer, 0, n);
					Assert.AreEqual ("is some text...", text);

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

					Assert.AreEqual (buffer.Length, bounded.Length);

					bounded.Read (buf, 0, buf.Length); // read the text
					bounded.Read (buf, 0, buf.Length); // cause eos to be true

					Assert.AreEqual (buffer.Length, bounded.Length);

					bounded.SetLength (500);

					Assert.AreEqual (500, bounded.Length);
					Assert.AreEqual (500, memory.Length);
				}
			}

			using (var memory = new MemoryStream ()) {
				var buffer = Encoding.ASCII.GetBytes ("This is some text...");

				memory.Write (buffer, 0, buffer.Length);

				using (var bounded = new BoundStream (memory, 0, buffer.Length, true)) {
					Assert.AreEqual (buffer.Length, bounded.Length);

					bounded.SetLength (500);

					Assert.AreEqual (500, bounded.Length);
					Assert.AreEqual (500, memory.Length);
				}
			}

			using (var memory = new MemoryStream ()) {
				var buffer = Encoding.ASCII.GetBytes ("This is some text...");

				memory.Write (buffer, 0, buffer.Length);

				using (var bounded = new BoundStream (memory, 0, buffer.Length, true)) {
					Assert.AreEqual (buffer.Length, bounded.Length);

					bounded.SetLength (5);

					Assert.AreEqual (5, bounded.Length);
					Assert.AreEqual (buffer.Length, memory.Length);
				}
			}
		}
	}
}
