//
// MimeParserTests.cs
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
using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MimeParserTests
	{
		[Test]
		public void TestSimpleMbox ()
		{
			using (var stream = File.OpenRead ("TestData/mbox/simple-mbox.txt")) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				while (!parser.IsEndOfStream) {
					var message = parser.ParseMessage ();
					Multipart multipart;
					MimeEntity entity;

					Assert.IsInstanceOfType (typeof (Multipart), message.Body);
					multipart = (Multipart) message.Body;
					Assert.AreEqual (1, multipart.Count);
					entity = multipart[0];

					Assert.IsInstanceOfType (typeof (Multipart), entity);
					multipart = (Multipart) entity;
					Assert.AreEqual (1, multipart.Count);
					entity = multipart[0];

					Assert.IsInstanceOfType (typeof (Multipart), entity);
					multipart = (Multipart) entity;
					Assert.AreEqual (1, multipart.Count);
					entity = multipart[0];

					Assert.IsInstanceOfType (typeof (TextPart), entity);

					using (var memory = new MemoryStream ()) {
						entity.WriteTo (memory);

						var text = Encoding.ASCII.GetString (memory.ToArray ());
						Assert.IsTrue (text.StartsWith ("Content-Type: text/plain\n\n"), "Headers are not properly terminated.");
					}
				}
			}
		}
	}
}
