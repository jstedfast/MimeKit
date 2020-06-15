//
// ContentObjectTests.cs
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
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class ContentObjectTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var content = new MimeContent (new MemoryStream ());

			Assert.Throws<ArgumentNullException> (() => new MimeContent (null));
			Assert.Throws<ArgumentNullException> (() => content.WriteTo (null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await content.WriteToAsync (null));
			Assert.Throws<ArgumentNullException> (() => content.DecodeTo (null));
			Assert.ThrowsAsync<ArgumentNullException> (async () => await content.DecodeToAsync (null));
		}

		[Test]
		public void TestCancellation ()
		{
			var content = new MimeContent (new MemoryStream (new byte[1024], false));

			using (var source = new CancellationTokenSource ()) {
				source.Cancel ();

				using (var dest = new MemoryStream ()) {
					Assert.Throws<OperationCanceledException> (() => content.WriteTo (dest, source.Token));
					Assert.AreEqual (0, dest.Length);

					Assert.ThrowsAsync<TaskCanceledException> (async () => await content.WriteToAsync (dest, source.Token));
					Assert.AreEqual (0, dest.Length);
				}
			}
		}
	}
}
