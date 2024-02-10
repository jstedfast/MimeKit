//
// TextPreviewerTests.cs
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

using MimeKit;
using MimeKit.Text;

namespace UnitTests.Text {
	[TestFixture]
	public class TextPreviewerTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => TextPreviewer.GetPreviewText ((TextPart) null));
		}

		static TextPart CreateTextPart (string path, TextFormat format)
		{
			var text = File.ReadAllText (path);

			return new TextPart (format) { Text = text };
		}

		static void AssertPreviewText (string path, TextFormat format, string expected)
		{
			var body = CreateTextPart (path, format);
			string actual;

			actual = TextPreviewer.GetPreviewText (body);
			Assert.That (actual, Is.EqualTo (expected));
		}

		[Test]
		public void TestHomeDepotCheckInsideNOW ()
		{
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "homedepot-check-inside-now.html");
			const string expected = "FREE DELIVERY Appliance Purchases $396 or More";

			AssertPreviewText (path, TextFormat.Html, expected);
		}

		[Test]
		public void TestMimeKitHomepage ()
		{
			string expected = "Toggle navigation MimeKit Home About Help Documentation Donate \u00D7 Install with NuGet (recommended) NuGet PM> Install-Package MimeKit PM> Install-Package MailKit or Install via VS Package Management window. Direct Download ZIP fil\u2026";
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "mimekit.net.html");

			AssertPreviewText (path, TextFormat.Html, expected);
		}

		[Test]
		public void TestPlanetFitness ()
		{
			string expected = "Don’t miss our celebrity guest Monday evening";
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "planet-fitness.html");

			AssertPreviewText (path, TextFormat.Html, expected);
		}

		[Test]
		public void TestPlanetFitnessPlain ()
		{
			const string expected = "Planet Fitness https://view.email.planetfitness.com/?qs=9a098a031cabde68c0a4260051cd6fe473a2e997a53678ff26b4b199a711a9d2ad0536530d6f837c246b09f644d42016ecfb298f930b7af058e9e454b34f3d818ceb3052ae317b1ac4594aab28a2d788 View web ver…";
			var path = Path.Combine (TestHelper.ProjectDir, "TestData", "text", "planet-fitness.txt");

			AssertPreviewText (path, TextFormat.Plain, expected);
		}

		[Test]
		public void TestGetPreviewTextUnknownCharset ()
		{
			var encoding = Encoding.GetEncoding ("big5");
			var builder = new StringBuilder ();

			using (var memory = new MemoryStream ()) {
				var bytes = encoding.GetBytes ("日月星辰");
				string preview;

				memory.Write (bytes, 0, bytes.Length);
				memory.Position = 0;

				var body = new TextPart ("plain") {
					Content = new MimeContent (memory, ContentEncoding.Default)
				};
				body.ContentType.Charset = "x-unknown";

				preview = TextPreviewer.GetPreviewText (body);
				Assert.That (preview, Is.EqualTo ("¤é¤ë¬P¨°"), "chinese text x-unknown -> UTF-8 -> iso-8859-1");
			}

			using (var memory = new MemoryStream ()) {
				var bytes = Encoding.UTF8.GetBytes ("日月星辰");
				string preview;

				memory.Write (bytes, 0, bytes.Length);
				memory.Position = 0;

				var body = new TextPart ("plain") {
					Content = new MimeContent (memory, ContentEncoding.Default)
				};
				body.ContentType.Charset = "x-unknown";

				preview = TextPreviewer.GetPreviewText (body);
				Assert.That (preview, Is.EqualTo ("日月星辰"), "x-unknown -> UTF-8");
			}

			using (var memory = new MemoryStream ()) {
				var bytes = Encoding.GetEncoding (28591).GetBytes ("L'encyclopédie libre");
				string preview;

				memory.Write (bytes, 0, bytes.Length);
				memory.Position = 0;

				var body = new TextPart ("plain") {
					Content = new MimeContent (memory, ContentEncoding.Default)
				};
				body.ContentType.Charset = "x-unknown";

				preview = TextPreviewer.GetPreviewText (body);
				Assert.That (preview, Is.EqualTo ("L'encyclopédie libre"), "french text x-unknown -> UTF-8 -> iso-8859-1");
			}
		}
	}
}
