﻿//
// MeasuringStreamTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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
using System.Threading.Tasks;

using NUnit.Framework;

using MimeKit.IO;

namespace UnitTests.IO {
	[TestFixture]
	public class MeasuringStreamTests
	{
		Random random = new Random ();

		[Test]
		public void TestCanReadWriteSeek ()
		{
			var buffer = new byte[1024];

			using (var block = new MeasuringStream ()) {
				Assert.IsFalse (block.CanRead);
				Assert.IsTrue (block.CanWrite);
				Assert.IsTrue (block.CanSeek);
				Assert.IsFalse (block.CanTimeout);
			}
		}

		[Test]
		public void TestGetSetTimeouts ()
		{
			using (var block = new MeasuringStream ()) {
				Assert.Throws<InvalidOperationException> (() => { int x = block.ReadTimeout; });
				Assert.Throws<InvalidOperationException> (() => { int x = block.WriteTimeout; });

				Assert.Throws<InvalidOperationException> (() => block.ReadTimeout = 5);
				Assert.Throws<InvalidOperationException> (() => block.WriteTimeout = 5);
			}
		}

		[Test]
		public void TestWrite ()
		{
			var buffer = new byte[1099];

			random.NextBytes (buffer);

			using (var stream = new MeasuringStream ()) {
				stream.Write (buffer, 0, buffer.Length);
				stream.Flush ();

				Assert.AreEqual (buffer.Length, stream.Length);
			}
		}

		[Test]
		public async Task TestWriteAsync ()
		{
			var buffer = new byte[1099];

			random.NextBytes (buffer);

			using (var stream = new MeasuringStream ()) {
				await stream.WriteAsync (buffer, 0, buffer.Length);
				await stream.FlushAsync ();

				Assert.AreEqual (buffer.Length, stream.Length);
			}
		}

		[Test]
		public void TestSeek ()
		{
			using (var stream = new MeasuringStream ()) {
				stream.SetLength (1024);

				for (int attempt = 0; attempt < 10; attempt++) {
					long offset = random.Next () % stream.Length;

					stream.Position = offset;

					long actual = stream.Position;
					long expected = offset;

					Assert.AreEqual (expected, actual, "SeekOrigin.Begin");
					Assert.AreEqual (expected, stream.Position, "Position");

					if (offset > 0) {
						// seek backwards from current position
						offset = -1 * (random.Next () % offset);
						expected += offset;

						actual = stream.Seek (offset, SeekOrigin.Current);

						Assert.AreEqual (expected, actual, "SeekOrigin.Current (-)");
						Assert.AreEqual (expected, stream.Position, "Position");
					}

					if (actual < stream.Length) {
						// seek forwards from current position
						offset = random.Next () % (stream.Length - actual);
						expected += offset;

						actual = stream.Seek (offset, SeekOrigin.Current);

						Assert.AreEqual (expected, actual, "SeekOrigin.Current (+)");
						Assert.AreEqual (expected, stream.Position, "Position");
					}

					// seek backwards from the end of the stream
					offset = -1 * (random.Next () % stream.Length);
					expected = stream.Length + offset;

					actual = stream.Seek (offset, SeekOrigin.End);

					Assert.AreEqual (expected, actual, "SeekOrigin.End");
					Assert.AreEqual (expected, stream.Position, "Position");
				}

				Assert.Throws<IOException> (() => stream.Seek (-1, SeekOrigin.Begin));
			}
		}

		[Test]
		public void TestSetLength ()
		{
			using (var stream = new MeasuringStream ()) {
				Assert.Throws<ArgumentOutOfRangeException> (() => stream.SetLength (-1));

				stream.SetLength (1024);

				Assert.AreEqual (1024, stream.Length);
			}
		}
	}
}
