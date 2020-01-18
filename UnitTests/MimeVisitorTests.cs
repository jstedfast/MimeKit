//
// MimeVisitorTests.cs
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
using System.Text;

using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MimeVisitorTests
	{
		[Test]
		public void TestMimeVisitor ()
		{
			var dataDir = Path.Combine ("..", "..", "TestData", "mbox");
			var visitor = new HtmlPreviewVisitor ();
			int index = 0;

			using (var stream = File.OpenRead (Path.Combine (dataDir, "jwz.mbox.txt"))) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				while (!parser.IsEndOfStream) {
					var filename = string.Format ("jwz.body.{0}.html", index);
					var path = Path.Combine (dataDir, filename);
					var message = parser.ParseMessage ();
					string expected, actual;

					visitor.Visit (message);

					actual = visitor.HtmlBody;

					if (!string.IsNullOrEmpty (actual))
						actual = actual.Replace ("\r\n", "\n");

					if (!File.Exists (path) && actual != null)
						File.WriteAllText (path, actual);

					if (File.Exists (path))
						expected = File.ReadAllText (path, Encoding.UTF8).Replace ("\r\n", "\n");
					else
						expected = null;

					if (index != 6 && index != 13 && index != 31) {
						// message 6, 13 and 31 contain some japanese text that is broken in Mono
						Assert.AreEqual (expected, actual, "The bodies do not match for message {0}", index);
					}

					visitor.Reset ();
					index++;
				}
			}
		}
	}
}
