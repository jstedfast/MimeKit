﻿//
// PackedByteArrayTests.cs
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

using NUnit.Framework;

using MimeKit.Utils;

namespace UnitTests.Utils {
	[TestFixture]
	public class PackedByteArrayTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var packed = new PackedByteArray ();

			Assert.Throws<ArgumentNullException> (() => packed.CopyTo (null, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => packed.CopyTo (new byte[16], -1));
		}

		[Test]
		public void TestBasicFunctionality ()
		{
			var packed = new PackedByteArray ();
			var expected = new byte[1024];
			var buffer = new byte[1024];
			int index = 0;

			for (int i = 0; i < 257; i++) {
				expected[index++] = (byte) 'A';
				packed.Add ((byte) 'A');
			}

			for (int i = 1; i < 26; i++) {
				expected[index++] = (byte) ('A' + i);
				packed.Add ((byte) ('A' + i));
			}

			for (int i = 0; i < 128; i++) {
				expected[index++] = (byte) 'B';
				packed.Add ((byte) 'B');
			}

			Assert.AreEqual (index, packed.Count, "Count");

			packed.CopyTo (buffer, 0);

			for (int i = 0; i < index; i++)
				Assert.AreEqual (expected[i], buffer[i], "buffer[{0}]", i);
		}
	}
}
