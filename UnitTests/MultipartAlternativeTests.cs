//
// MultipartAlternativeTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2023 .NET Foundation and Contributors
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

using MimeKit;
using MimeKit.Text;

namespace UnitTests {
	[TestFixture]
	public class MultipartAlternativeTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var alternative = new MultipartAlternative ();

			Assert.Throws<ArgumentNullException> (() => new MultipartAlternative ((MimeEntityConstructorArgs) null));
			Assert.Throws<ArgumentNullException> (() => alternative.Accept (null));
		}

		[Test]
		public void TestGenericArgsConstructor ()
		{
			var multipart = new MultipartAlternative (
				new Header (HeaderId.ContentDescription, "This is a description of the multipart."),
				new TextPart (TextFormat.Plain) { Text = "This is the message body." },
				new MimePart ("image", "gif") { FileName = "attachment.gif" }
				);

			Assert.IsTrue (multipart.Headers.Contains (HeaderId.ContentDescription), "Content-Description header");
			Assert.AreEqual (2, multipart.Count, "Child part count");
			Assert.AreEqual ("text/plain", multipart[0].ContentType.MimeType, "MimeType[0]");
			Assert.AreEqual ("image/gif", multipart[1].ContentType.MimeType, "MimeType[1]");
		}

		[Test]
		public void TestGetTextBody ()
		{
			var alternative = new MultipartAlternative ();
			var plain = new TextPart ("plain") { Text = "plain\n" };
			var flowed = new TextPart (TextFormat.Flowed) { Text = "flowed\n" };
			var richtext = new TextPart ("rtf") { Text = "rtf\n" };
			var html = new TextPart ("html") { Text = "html\n" };

			alternative.Add (plain);
			alternative.Add (richtext);
			alternative.Add (html);

			Assert.AreEqual ("plain\n", alternative.TextBody.Replace ("\r\n", "\n"), "TextBody");
			Assert.AreEqual ("html\n", alternative.HtmlBody.Replace ("\r\n", "\n"), "HtmlBody");

			alternative.Insert (1, flowed);

			// Note: GetTextBody (Plain) returns Flowed because Flowed is also Plain and is listed after the text/plain part
			Assert.AreEqual ("flowed\n", alternative.GetTextBody (TextFormat.Plain).Replace ("\r\n", "\n"), "Plain");
			Assert.AreEqual ("flowed\n", alternative.GetTextBody (TextFormat.Flowed).Replace ("\r\n", "\n"), "Flowed");
			Assert.AreEqual ("rtf\n", alternative.GetTextBody (TextFormat.RichText).Replace ("\r\n", "\n"), "RichText");
			Assert.AreEqual ("html\n", alternative.GetTextBody (TextFormat.Html).Replace ("\r\n", "\n"), "Html");
			Assert.IsNull (alternative.GetTextBody (TextFormat.Enriched), "Enriched");
		}

		[Test]
		public void TestGetTextBodyNestedAlternatives ()
		{
			var alternative = new MultipartAlternative ();
			var plain = new TextPart ("plain") { Text = "plain\n" };
			var flowed = new TextPart (TextFormat.Flowed) { Text = "flowed\n" };
			var richtext = new TextPart ("rtf") { Text = "rtf\n" };
			var html = new TextPart ("html") { Text = "html\n" };

			alternative.Add (plain);
			alternative.Add (richtext);
			alternative.Add (html);

			var outer = new MultipartAlternative {
				alternative
			};

			Assert.AreEqual ("plain\n", outer.TextBody.Replace ("\r\n", "\n"), "TextBody");
			Assert.AreEqual ("html\n", outer.HtmlBody.Replace ("\r\n", "\n"), "HtmlBody");

			alternative.Insert (1, flowed);

			// Note: GetTextBody (Plain) returns Flowed because Flowed is also Plain and is listed after the text/plain part
			Assert.AreEqual ("flowed\n", outer.GetTextBody (TextFormat.Plain).Replace ("\r\n", "\n"), "Plain");
			Assert.AreEqual ("flowed\n", outer.GetTextBody (TextFormat.Flowed).Replace ("\r\n", "\n"), "Flowed");
			Assert.AreEqual ("rtf\n", outer.GetTextBody (TextFormat.RichText).Replace ("\r\n", "\n"), "RichText");
			Assert.AreEqual ("html\n", outer.GetTextBody (TextFormat.Html).Replace ("\r\n", "\n"), "Html");
			Assert.IsNull (outer.GetTextBody (TextFormat.Enriched), "Enriched");
		}

		[Test]
		public void TestGetTextBodyAlternativeInsideRelated ()
		{
			var alternative = new MultipartAlternative ();
			var plain = new TextPart ("plain") { Text = "plain\n" };
			var flowed = new TextPart (TextFormat.Flowed) { Text = "flowed\n" };
			var richtext = new TextPart ("rtf") { Text = "rtf\n" };
			var html = new TextPart ("html") { Text = "html\n" };

			alternative.Add (plain);
			alternative.Add (richtext);
			alternative.Add (html);

			var related = new MultipartRelated {
				alternative
			};

			var outer = new MultipartAlternative {
				related
			};

			Assert.AreEqual ("plain\n", outer.TextBody.Replace ("\r\n", "\n"), "TextBody");
			Assert.AreEqual ("html\n", outer.HtmlBody.Replace ("\r\n", "\n"), "HtmlBody");

			alternative.Insert (1, flowed);

			// Note: GetTextBody (Plain) returns Flowed because Flowed is also Plain and is listed after the text/plain part
			Assert.AreEqual ("flowed\n", outer.GetTextBody (TextFormat.Plain).Replace ("\r\n", "\n"), "Plain");
			Assert.AreEqual ("flowed\n", outer.GetTextBody (TextFormat.Flowed).Replace ("\r\n", "\n"), "Flowed");
			Assert.AreEqual ("rtf\n", outer.GetTextBody (TextFormat.RichText).Replace ("\r\n", "\n"), "RichText");
			Assert.AreEqual ("html\n", outer.GetTextBody (TextFormat.Html).Replace ("\r\n", "\n"), "Html");
			Assert.IsNull (outer.GetTextBody (TextFormat.Enriched), "Enriched");
		}

		[Test]
		public void TestGetTextBodyMixedInsideAlternative ()
		{
			var mixed = new Multipart ("mixed");
			var plain = new TextPart ("plain") { Text = "plain\n" };
			var flowed = new TextPart (TextFormat.Flowed) { Text = "flowed\n" };
			var richtext = new TextPart ("rtf") { Text = "rtf\n" };
			var html = new TextPart ("html") { Text = "html\n" };

			mixed.Add (plain);
			mixed.Add (richtext);
			mixed.Add (html);

			var alternative = new MultipartAlternative {
				mixed
			};

			Assert.AreEqual ("plain\n", alternative.TextBody.Replace ("\r\n", "\n"), "TextBody");
			//Assert.AreEqual ("html\n", alternative.HtmlBody.Replace ("\r\n", "\n"), "HtmlBody");

			mixed.Insert (1, flowed);

			// Note: Only the text/plain part will be found because Multipart.TryGetValue() will only look at the very first text part.
			Assert.AreEqual ("plain\n", alternative.GetTextBody (TextFormat.Plain).Replace ("\r\n", "\n"), "Plain");
			Assert.IsNull (alternative.GetTextBody (TextFormat.Flowed), "Flowed");
			Assert.IsNull (alternative.GetTextBody (TextFormat.RichText), "RichText");
			Assert.IsNull (alternative.GetTextBody (TextFormat.Html), "Html");
			Assert.IsNull (alternative.GetTextBody (TextFormat.Enriched), "Enriched");
		}
	}
}
