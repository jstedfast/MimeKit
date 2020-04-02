﻿//
// TnefReaderStreamTests.cs
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

using NUnit.Framework;

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefReaderStreamTests
	{
		[Test]
		public void TestTnefReaderStream ()
		{
			using (var stream = File.OpenRead ("../../TestData/tnef/winmail.tnef")) {
				using (var reader = new TnefReader (stream)) {
					var buffer = new byte[1024];

					using (var tnef = new TnefReaderStream (reader, 0, 0)) {
						Assert.IsTrue (tnef.CanRead);
						Assert.IsFalse (tnef.CanWrite);
						Assert.IsFalse (tnef.CanSeek);
						Assert.IsFalse (tnef.CanTimeout);

						Assert.Throws<ArgumentNullException> (() => tnef.Read (null, 0, buffer.Length));
						Assert.Throws<ArgumentOutOfRangeException> (() => tnef.Read (buffer, -1, buffer.Length));
						Assert.Throws<ArgumentOutOfRangeException> (() => tnef.Read (buffer, 0, -1));

						Assert.Throws<NotSupportedException> (() => tnef.Write (buffer, 0, buffer.Length));
						Assert.Throws<NotSupportedException> (() => tnef.Seek (0, SeekOrigin.End));
						Assert.Throws<NotSupportedException> (() => tnef.Flush ());
						Assert.Throws<NotSupportedException> (() => tnef.SetLength (1024));

						Assert.Throws<NotSupportedException> (() => { var x = tnef.Position; });
						Assert.Throws<NotSupportedException> (() => { tnef.Position = 0; });
						Assert.Throws<NotSupportedException> (() => { var x = tnef.Length; });
					}
				}
			}
		}
	}
}
