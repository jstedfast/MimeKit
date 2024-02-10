//
// HtmlTextPreviewerTests.cs
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

using MimeKit.Text;

namespace UnitTests.Text {
	[TestFixture]
	public class HtmlTextPreviewerTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var previewer = new HtmlTextPreviewer ();

			Assert.Throws<ArgumentOutOfRangeException> (() => previewer.MaximumPreviewLength = 0);
			Assert.Throws<ArgumentOutOfRangeException> (() => previewer.MaximumPreviewLength = 1025);

			Assert.Throws<ArgumentNullException> (() => previewer.GetPreviewText ((string) null));
			Assert.Throws<ArgumentNullException> (() => previewer.GetPreviewText ((TextReader) null));
			Assert.Throws<ArgumentNullException> (() => previewer.GetPreviewText ((Stream) null, "charset"));
			Assert.Throws<ArgumentNullException> (() => previewer.GetPreviewText (Stream.Null, (string) null));
			Assert.Throws<ArgumentNullException> (() => previewer.GetPreviewText ((Stream) null, Encoding.ASCII));
			Assert.Throws<ArgumentNullException> (() => previewer.GetPreviewText (Stream.Null, (Encoding) null));
		}

		[Test]
		public void TestEmptyText ()
		{
			var previewer = new HtmlTextPreviewer ();

			Assert.That (previewer.GetPreviewText (string.Empty), Is.EqualTo (string.Empty), "string");

			using (var reader = new StringReader (string.Empty))
				Assert.That (previewer.GetPreviewText (reader), Is.EqualTo (string.Empty), "TextReader");

			using (var stream = new MemoryStream (Array.Empty<byte> (), false)) {
				Assert.That (previewer.GetPreviewText (stream, "x-unknown"), Is.EqualTo (string.Empty), "Stream, string");
				Assert.That (previewer.GetPreviewText (stream, Encoding.UTF8), Is.EqualTo (string.Empty), "Stream, Encoding");
			}
		}

		static void AssertPreviewText (string path, string expected, int maxPreviewLen)
		{
			var previewer = new HtmlTextPreviewer { MaximumPreviewLength = maxPreviewLen };
			var buffer = new byte[16 * 1024];
			string actual;
			int nread;

			Assert.That (previewer.InputFormat, Is.EqualTo (TextFormat.Html));

			using (var stream = File.OpenRead (path))
				nread = stream.Read (buffer, 0, buffer.Length);

			var text = Encoding.UTF8.GetString (buffer, 0, nread);
			actual = previewer.GetPreviewText (text);
			Assert.That (actual, Is.EqualTo (expected), "GetPreviewText(string)");

			using (var stream = new MemoryStream (buffer, 0, nread, false)) {
				actual = previewer.GetPreviewText (stream, "utf-8");
				Assert.That (actual, Is.EqualTo (expected), "GetPreviewText(Stream, string)");

				stream.Position = 0;
				actual = previewer.GetPreviewText (stream, Encoding.UTF8);
				Assert.That (actual, Is.EqualTo (expected), "GetPreviewText(Stream, Encoding)");

				stream.Position = 0;
				using (var reader = new StreamReader (stream, Encoding.UTF8, false, 4096, true)) {
					actual = previewer.GetPreviewText (stream, Encoding.UTF8);
					Assert.That (actual, Is.EqualTo (expected), "GetPreviewText(TextReader)");
				}
			}
		}

		[Test]
		public void TestHomeDepot110 ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "homedepot-check-inside-now.html");
			const string expected = "FREE DELIVERY Appliance Purchases $396 or More";

			AssertPreviewText (path, expected, 110);
		}

		[Test]
		public void TestHomeDepot230 ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "homedepot-check-inside-now.html");
			const string expected = "FREE DELIVERY Appliance Purchases $396 or More";

			AssertPreviewText (path, expected, 230);
		}

		[Test]
		public void TestMimeKitHomepage110 ()
		{
			string expected = "Toggle navigation MimeKit Home About Help Documentation Donate \u00D7 Install with NuGet (recommended) NuGet PM> I\u2026";
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "mimekit.net.html");

			AssertPreviewText (path, expected, 110);
		}

		[Test]
		public void TestMimeKitHomepage230 ()
		{
			string expected = "Toggle navigation MimeKit Home About Help Documentation Donate \u00D7 Install with NuGet (recommended) NuGet PM> Install-Package MimeKit PM> Install-Package MailKit or Install via VS Package Management window. Direct Download ZIP fil\u2026";
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "mimekit.net.html");

			AssertPreviewText (path, expected, 230);
		}

		[Test]
		public void TestPlanetFitness110 ()
		{
			string expected = "Don’t miss our celebrity guest Monday evening";
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "planet-fitness.html");

			AssertPreviewText (path, expected, 110);
		}

		[Test]
		public void TestPlanetFitness230 ()
		{
			string expected = "Don’t miss our celebrity guest Monday evening";
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "planet-fitness.html");

			AssertPreviewText (path, expected, 230);
		}
	}
}
