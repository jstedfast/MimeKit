//
// TextPreviewerTests.cs
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
	public class TextPreviewerTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => TextPreviewer.GetPreviewText (null));
		}

		static TextPart CreateTextPart (string path)
		{
			var subtype = Path.GetExtension (path) == ".html" ? "html" : "plain";
			var text = File.ReadAllText (path);

			return new TextPart (subtype) { Text = text };
		}

		void AssertPreviewText (string path, string expected)
		{
			var body = CreateTextPart (path);
			string actual;

			actual = TextPreviewer.GetPreviewText (body);
			Assert.AreEqual (expected, actual);
		}

		[Test]
		public void TestHomeDepotCheckInsideNOW ()
		{
			var path = Path.Combine ("..", "..", "TestData", "text", "homedepot-check-inside-now.html");
			const string expected = "FREE DELIVERY Appliance Purchases $396 or More";

			AssertPreviewText (path, expected);
		}

		[Test]
		public void TestMimeKitHomepage ()
		{
			string expected = "Toggle navigation MimeKit Home About Help Documentation Donate \u00D7 Install with NuGet (recommended) NuGet PM> I\u2026";
			var path = Path.Combine ("..", "..", "TestData", "text", "mimekit.net.html");

			AssertPreviewText (path, expected);
		}

		[Test]
		public void TestPlanetFitness ()
		{
			string expected = "Don’t miss our celebrity guest Monday evening";
			var path = Path.Combine ("..", "..", "TestData", "text", "planet-fitness.html");

			AssertPreviewText (path, expected);
		}

		[Test]
		public void TestPlanetFitnessPlain ()
		{
			const string expected = "Planet Fitness https://view.email.planetfitness.com/?qs=9a098a031cabde68c0a4260051cd6fe473a2e997a53678ff26b4b…";
			var path = Path.Combine ("..", "..", "TestData", "text", "planet-fitness.txt");

			AssertPreviewText (path, expected);
		}
	}
}
