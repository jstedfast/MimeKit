//
// YEncodingTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (www.xamarin.com)
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

using MimeKit;
using MimeKit.IO;
using MimeKit.IO.Filters;
using MimeKit.Encodings;

namespace UnitTests {
	[TestFixture]
	public class YEncodingTests
	{
		[Test]
		public void TestDecodeSimpleYEncMessage ()
		{
			using (var file = File.OpenRead ("../../TestData/yenc/simple.msg")) {
				var message = MimeMessage.Load (file);

				using (var decoded = new MemoryStream ()) {
					var ydec = new YDecoder ();

					using (var filtered = new FilteredStream (decoded)) {
						filtered.Add (new DecoderFilter (ydec));

						((MimePart) message.Body).ContentObject.WriteTo (filtered);
					}

					Assert.AreEqual (584, decoded.Length, "The decoded size does not match.");
					Assert.AreEqual (0xded29f4f, ydec.Checksum ^ 0xffffffff, "The checksum does not match.");
				}
			}
		}
	}
}
