//
// HtmlPreviewVisitorTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using NUnit.Framework;

using MimeKit;

namespace MessageReader.Tests {
	[TestFixture]
	public class HtmlPreviewVisitorTests
	{
		const string Pixel = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a8XcAAAAASUVORK5CYII=";

		static IHtmlDocument Preview (MimeEntity body)
		{
			using (var message = new MimeMessage { Body = body }) {
				var visitor = new HtmlPreviewVisitor ();
				message.Accept (visitor);
				return new HtmlParser ().ParseDocument (visitor.HtmlBody);
			}
		}

		static IHtmlDocument Preview (string html)
		{
			return Preview (new TextPart ("html") { Text = html });
		}

		static MimePart CreateImage (string mimeType = "image/png")
		{
			return new MimePart (mimeType) {
				ContentId = "picture",
				Content = new MimeContent (new MemoryStream (Convert.FromBase64String (Pixel)))
			};
		}

		[Test]
		public void TestActiveContentAndNavigationAreRemoved ()
		{
			const string html = "<html><head><base href='https://example.test/'>" +
				"<meta http-equiv='refresh' content='0;url=https://example.test/'>" +
				"<link rel='stylesheet' href='https://example.test/style.css'></head>" +
				"<body onload='alert(1)'><script>alert(1)</script>" +
				"<iframe srcdoc='<script>alert(1)</script>'></iframe>" +
				"<object data='https://example.test/object'></object>" +
				"<svg onload='alert(1)'></svg><math></math><form action='https://example.test/'><input></form>" +
				"<a href='https://example.test/' target='_blank' ping='https://example.test/ping'>link</a>" +
				"<p onclick='alert(1)'>Hello</p></body></html>";

			using (var document = Preview (html)) {
				Assert.That (document.QuerySelectorAll ("script, iframe, object, svg, math, form, input, meta, link, base"), Is.Empty);
				Assert.That (document.QuerySelectorAll ("[onload], [onclick], [href], [target], [ping]"), Is.Empty);
				Assert.That (document.QuerySelector ("a").TextContent, Is.EqualTo ("link"));
				Assert.That (document.QuerySelector ("p").TextContent, Is.EqualTo ("Hello"));
			}
		}

		[TestCase ("https://example.test/pixel.png")]
		[TestCase ("http://example.test/pixel.png")]
		[TestCase ("//example.test/pixel.png")]
		[TestCase ("/pixel.png")]
		[TestCase ("pixel.png")]
		[TestCase ("file:///C:/pixel.png")]
		[TestCase ("cid:missing")]
		[TestCase ("javascript:alert(1)")]
		[TestCase ("java&#x09;script:alert(1)")]
		[TestCase ("data:image/png;base64," + Pixel)]
		[TestCase ("data:image/svg+xml,%3Csvg%20onload=alert(1)%3E")]
		[TestCase ("")]
		public void TestUnresolvedImagesAreBlocked (string url)
		{
			using (var document = Preview ("<img src=\"" + url + "\" srcset='https://example.test/other.png 2x' alt='photo' onerror='alert(1)'>")) {
				var image = document.QuerySelector ("img");
				Assert.That (image.HasAttribute ("src"), Is.False);
				Assert.That (image.HasAttribute ("srcset"), Is.False);
				Assert.That (image.HasAttribute ("onerror"), Is.False);
				Assert.That (image.GetAttribute ("alt"), Is.EqualTo ("photo"));
			}
		}

		[TestCase ("cid:picture")]
		[TestCase ("cid:pi&#99;ture")]
		[TestCase ("images/picture.png")]
		[TestCase ("https://example.test/images/picture.png")]
		public void TestRelatedRasterImages (string url)
		{
			var image = CreateImage ();
			if (!url.StartsWith ("cid:", StringComparison.Ordinal))
				image.ContentLocation = new Uri (url, UriKind.RelativeOrAbsolute);

			var related = new MultipartRelated {
				new TextPart ("html") { Text = "<img src='" + url + "' alt='photo' width='32' height='24'>" },
				image
			};

			using (var document = Preview (related)) {
				var img = document.QuerySelector ("img");
				Assert.That (img.GetAttribute ("src"), Is.EqualTo ("data:image/png;base64," + Pixel));
				Assert.That (img.GetAttribute ("alt"), Is.EqualTo ("photo"));
				Assert.That (img.GetAttribute ("width"), Is.EqualTo ("32"));
				Assert.That (img.GetAttribute ("height"), Is.EqualTo ("24"));
			}
		}

		[TestCase ("image/svg+xml")]
		[TestCase ("text/html")]
		[TestCase ("application/octet-stream")]
		public void TestNonRasterRelatedPartsAreBlocked (string mimeType)
		{
			var related = new MultipartRelated {
				new TextPart ("html") { Text = "<img src='cid:picture'>" },
				CreateImage (mimeType)
			};

			using (var document = Preview (related))
				Assert.That (document.QuerySelector ("img").HasAttribute ("src"), Is.False);
		}

		[Test]
		public void TestNestedRelatedImageScope ()
		{
			var innerImage = CreateImage ();
			var outerImage = CreateImage ("image/svg+xml");
			var inner = new MultipartRelated {
				new TextPart ("html") { Text = "<img src='cid:picture'>" },
				innerImage
			};
			var outer = new MultipartRelated { inner, outerImage };

			using (var document = Preview (outer))
				Assert.That (document.QuerySelector ("img").GetAttribute ("src"), Is.EqualTo ("data:image/png;base64," + Pixel));
		}

		[Test]
		public void TestFormattingIsPreservedWithoutCssResources ()
		{
			const string html = "<head><style>@import url('https://example.test/import.css');" +
				"@font-face {font-family: tracking; src: url('https://example.test/font.woff');}" +
				".message {color: blue; background-image: url('https://example.test/pixel.png');}" +
				"@media screen {.message {font-weight: bold;}}</style></head>" +
				"<body background='https://example.test/body.png'><table cellpadding='4'><tr><td class='message' " +
				"style=\"color: red; padding: 5px; background-image: url('https://example.test/bg.png');" +
				"background: image-set('https://example.test/set.png' 1x);" +
				"cursor: url('https://example.test/cursor.cur'), auto; --image: url('https://example.test/var.png');" +
				"width: expression(alert(1)); behavior: url('https://example.test/behavior.htc');\">" +
				"R\u00e9sum\u00e9 \u65e5\u672c\u8a9e</td></tr></table></body>";

			using (var document = Preview (html)) {
				var cell = document.QuerySelector ("td");
				Assert.That (cell.GetAttribute ("class"), Is.EqualTo ("message"));
				Assert.That (cell.GetAttribute ("style"), Does.Contain ("color:").And.Contain ("padding"));
				Assert.That (cell.TextContent, Is.EqualTo ("R\u00e9sum\u00e9 \u65e5\u672c\u8a9e"));
				Assert.That (document.QuerySelector ("table").GetAttribute ("cellpadding"), Is.EqualTo ("4"));
				var css = document.QuerySelector ("style").TextContent;
				Assert.That (css, Does.Contain (".message").And.Contain ("color:").And.Contain ("@media").And.Contain ("font-weight:"));
				Assert.That (document.DocumentElement.OuterHtml, Does.Not.Contain ("example.test").And.Not.Contain ("expression(").And.Not.Contain ("@font-face").And.Not.Contain ("@import"));
			}
		}

		[TestCase ("plain")]
		[TestCase ("plain; format=flowed")]
		public void TestTextBodiesAreEncoded (string format)
		{
			var text = new TextPart ("plain") { Text = "<script>alert(1)</script>\r\nHello & goodbye" };
			if (format.Contains ("flowed")) {
				text.ContentType.Parameters["format"] = "flowed";
				text.ContentType.Parameters["delsp"] = "yes";
			}

			using (var document = Preview (text)) {
				Assert.That (document.QuerySelectorAll ("script"), Is.Empty);
				Assert.That (document.Body.TextContent, Does.Contain ("<script>alert(1)</script>").And.Contain ("Hello & goodbye"));
			}
		}

		[Test]
		public void TestHtmlAlternativeIsSelected ()
		{
			var alternative = new MultipartAlternative {
				new TextPart ("plain") { Text = "plain alternative" },
				new TextPart ("html") { Text = "<b>HTML alternative</b><script>alert(1)</script>" }
			};

			using (var document = Preview (alternative)) {
				Assert.That (document.QuerySelector ("b").TextContent, Is.EqualTo ("HTML alternative"));
				Assert.That (document.QuerySelectorAll ("script"), Is.Empty);
				Assert.That (document.Body.TextContent, Does.Not.Contain ("plain alternative"));
			}
		}

		[TestCase ("<svg><style><img src=x onerror='alert(1)'></style></svg><p>body</p>")]
		[TestCase ("<math><mtext><table><mglyph><style><!--</style><img src=x onerror='alert(1)'>")]
		[TestCase ("<noscript><p title='</noscript><img src=x onerror=alert(1)>'>body</p>")]
		public void TestMalformedForeignContent (string html)
		{
			using (var document = Preview (html)) {
				Assert.That (document.QuerySelectorAll ("svg, math, script, noscript, [onerror]"), Is.Empty);
				Assert.That (document.QuerySelectorAll ("img[src]"), Is.Empty);
			}
		}

		[Test]
		public void TestSampleMessage ()
		{
			using (var stream = typeof (MessageViewWindow).Assembly.GetManifestResourceStream ("MessageReader.xamarin3.eml"))
			using (var message = MimeMessage.Load (stream)) {
				var visitor = new HtmlPreviewVisitor ();
				message.Accept (visitor);

				using (var document = new HtmlParser ().ParseDocument (visitor.HtmlBody)) {
					Assert.That (document.Body.TextContent, Is.Not.Empty);
					Assert.That (document.QuerySelectorAll ("script, iframe, meta, base, link, [href], [srcset]"), Is.Empty);
					foreach (var image in document.QuerySelectorAll ("img[src]"))
						Assert.That (image.GetAttribute ("src"), Does.StartWith ("data:image/"));
				}
			}
		}

		static void LoadPreview (MessageViewWindow window, WebBrowser browser, MimeMessage message)
		{
			bool completed = false;
			WebBrowserDocumentCompletedEventHandler handler = (sender, e) => completed = true;
			browser.DocumentCompleted += handler;

			try {
				window.Message = message;
				var timer = Stopwatch.StartNew ();
				while (!completed && timer.Elapsed < TimeSpan.FromSeconds (10)) {
					Application.DoEvents ();
					Thread.Sleep (10);
				}
				Assert.That (completed, Is.True, "preview document loaded");
			} finally {
				browser.DocumentCompleted -= handler;
			}
		}

		[Test]
		[Apartment (ApartmentState.STA)]
		public void TestBrowserLoadsSanitizedMessages ()
		{
			using (var window = new MessageViewWindow ())
			using (var first = new MimeMessage {
				Body = new MultipartRelated {
					new TextPart ("html") {
						Text = "<body onload=\"document.title='executed'\"><p>R\u00e9sum\u00e9 \u65e5\u672c\u8a9e</p>" +
							"<script>document.title='executed'</script><img src='cid:picture'></body>"
					},
					CreateImage ()
				}
			})
			using (var second = new MimeMessage { Body = new TextPart ("plain") { Text = "second message" } }) {
				var browser = (WebBrowser) window.Controls[0];
				Assert.That (browser.Handle, Is.Not.EqualTo (IntPtr.Zero));

				LoadPreview (window, browser, first);
				Assert.That (browser.Document.Body.InnerText, Does.Contain ("R\u00e9sum\u00e9 \u65e5\u672c\u8a9e"));
				Assert.That (browser.DocumentTitle, Is.Not.EqualTo ("executed"));
				Assert.That (browser.Document.GetElementsByTagName ("script").Count, Is.Zero);
				Assert.That (browser.Document.Images.Count, Is.EqualTo (1));
				Assert.That (browser.Document.Images[0].GetAttribute ("src"), Does.StartWith ("data:image/png;base64,"));
				Assert.That (browser.Document.Images[0].GetAttribute ("width"), Is.EqualTo ("1"), "embedded image decoded");
				Assert.That (browser.AllowWebBrowserDrop, Is.False);

				LoadPreview (window, browser, second);
				Assert.That (browser.Document.Body.InnerText, Does.Contain ("second message"));
				AssertNavigationIsBlocked (browser);
			}
		}

		static void AssertNavigationIsBlocked (WebBrowser browser)
		{
			bool canceled = false;
			WebBrowserNavigatingEventHandler handler = (sender, e) => canceled = e.Cancel;
			browser.Navigating += handler;

			try {
				browser.Navigate ("about:blank#blocked");
				var timer = Stopwatch.StartNew ();
				while (!canceled && timer.Elapsed < TimeSpan.FromSeconds (2)) {
					Application.DoEvents ();
					Thread.Sleep (10);
				}
				Assert.That (canceled, Is.True, "navigation canceled");
			} finally {
				browser.Navigating -= handler;
			}
		}

		[Test]
		[Apartment (ApartmentState.STA)]
		public void TestBrowserNavigationIsDisabled ()
		{
			using (var window = new MessageViewWindow ()) {
				var browser = (WebBrowser) window.Controls[0];
				Assert.That (browser.AllowWebBrowserDrop, Is.False);
				Assert.That (browser.IsWebBrowserContextMenuEnabled, Is.False);
				Assert.That (browser.WebBrowserShortcutsEnabled, Is.False);
				AssertNavigationIsBlocked (browser);
			}
		}
	}
}
