//
// PlainTextPreviewerTests.cs
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

using MimeKit;
using MimeKit.Text;

using NUnit.Framework;

namespace UnitTests.Text {
	[TestFixture]
	public class PlainTextPreviewerTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var previewer = new PlainTextPreviewer ();

			Assert.Throws<ArgumentOutOfRangeException> (() => previewer.MaximumPreviewLength = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => previewer.MaximumPreviewLength = 1025);

			Assert.Throws<ArgumentNullException> (() => previewer.GetPreviewText ((string) null));
			Assert.Throws<ArgumentNullException> (() => previewer.GetPreviewText ((TextReader) null));
			Assert.Throws<ArgumentNullException> (() => previewer.GetPreviewText ((Stream) null, "charset"));
			Assert.Throws<ArgumentNullException> (() => previewer.GetPreviewText (Stream.Null, (string) null));
			Assert.Throws<ArgumentNullException> (() => previewer.GetPreviewText ((Stream) null, Encoding.ASCII));
			Assert.Throws<ArgumentNullException> (() => previewer.GetPreviewText (Stream.Null, (Encoding) null));
		}

		void AssertPreviewText (string path, string expected)
		{
			var previewer = new PlainTextPreviewer ();
			var buffer = new byte[16 * 1024];
			string actual;
			int nread;

			using (var stream = File.OpenRead (path))
				nread = stream.Read (buffer, 0, buffer.Length);

			var text = Encoding.UTF8.GetString (buffer, 0, nread);
			actual = previewer.GetPreviewText (text);
			Assert.AreEqual (expected, actual, "GetPreviewText(string)");

			using (var stream = new MemoryStream (buffer, 0, nread, false)) {
				actual = previewer.GetPreviewText (stream, "utf-8");
				Assert.AreEqual (expected, actual, "GetPreviewText(Stream, string)");

				stream.Position = 0;
				actual = previewer.GetPreviewText (stream, Encoding.UTF8);
				Assert.AreEqual (expected, actual, "GetPreviewText(Stream, Encoding)");

				stream.Position = 0;
				using (var reader = new StreamReader (stream, Encoding.UTF8, false, 4096, true)) {
					actual = previewer.GetPreviewText (stream, Encoding.UTF8);
					Assert.AreEqual (expected, actual, "GetPreviewText(TextReader)");
				}
			}
		}

		[Test]
		public void TestPlanetFitness ()
		{
			const string expected = "Planet Fitness https://view.email.planetfitness.com/?qs=9a098a031cabde68c0a4260051cd6fe473a2e997a53678ff26b4b…";
			var path = Path.Combine ("..", "..", "TestData", "text", "planet-fitness.txt");

			AssertPreviewText (path, expected);
		}
	}
}
