//
// MultipartAlternativeTests.cs
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

			Assert.That (multipart.Headers.Contains (HeaderId.ContentDescription), Is.True, "Content-Description header");
			Assert.That (multipart.Count, Is.EqualTo (2), "Child part count");
			Assert.That (multipart[0].ContentType.MimeType, Is.EqualTo ("text/plain"), "MimeType[0]");
			Assert.That (multipart[1].ContentType.MimeType, Is.EqualTo ("image/gif"), "MimeType[1]");
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

			Assert.That (alternative.TextBody.Replace ("\r\n", "\n"), Is.EqualTo ("plain\n"), "TextBody");
			Assert.That (alternative.HtmlBody.Replace ("\r\n", "\n"), Is.EqualTo ("html\n"), "HtmlBody");

			alternative.Insert (1, flowed);

			// Note: GetTextBody (Plain) returns Flowed because Flowed is also Plain and is listed after the text/plain part
			Assert.That (alternative.GetTextBody (TextFormat.Plain).Replace ("\r\n", "\n"), Is.EqualTo ("flowed\n"), "Plain");
			Assert.That (alternative.GetTextBody (TextFormat.Flowed).Replace ("\r\n", "\n"), Is.EqualTo ("flowed\n"), "Flowed");
			Assert.That (alternative.GetTextBody (TextFormat.RichText).Replace ("\r\n", "\n"), Is.EqualTo ("rtf\n"), "RichText");
			Assert.That (alternative.GetTextBody (TextFormat.Html).Replace ("\r\n", "\n"), Is.EqualTo ("html\n"), "Html");
			Assert.That (alternative.GetTextBody (TextFormat.Enriched), Is.Null, "Enriched");
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

			Assert.That (outer.TextBody.Replace ("\r\n", "\n"), Is.EqualTo ("plain\n"), "TextBody");
			Assert.That (outer.HtmlBody.Replace ("\r\n", "\n"), Is.EqualTo ("html\n"), "HtmlBody");

			alternative.Insert (1, flowed);

			// Note: GetTextBody (Plain) returns Flowed because Flowed is also Plain and is listed after the text/plain part
			Assert.That (outer.GetTextBody (TextFormat.Plain).Replace ("\r\n", "\n"), Is.EqualTo ("flowed\n"), "Plain");
			Assert.That (outer.GetTextBody (TextFormat.Flowed).Replace ("\r\n", "\n"), Is.EqualTo ("flowed\n"), "Flowed");
			Assert.That (outer.GetTextBody (TextFormat.RichText).Replace ("\r\n", "\n"), Is.EqualTo ("rtf\n"), "RichText");
			Assert.That (outer.GetTextBody (TextFormat.Html).Replace ("\r\n", "\n"), Is.EqualTo ("html\n"), "Html");
			Assert.That (outer.GetTextBody (TextFormat.Enriched), Is.Null, "Enriched");
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

			Assert.That (outer.TextBody.Replace ("\r\n", "\n"), Is.EqualTo ("plain\n"), "TextBody");
			Assert.That (outer.HtmlBody.Replace ("\r\n", "\n"), Is.EqualTo ("html\n"), "HtmlBody");

			alternative.Insert (1, flowed);

			// Note: GetTextBody (Plain) returns Flowed because Flowed is also Plain and is listed after the text/plain part
			Assert.That (outer.GetTextBody (TextFormat.Plain).Replace ("\r\n", "\n"), Is.EqualTo ("flowed\n"), "Plain");
			Assert.That (outer.GetTextBody (TextFormat.Flowed).Replace ("\r\n", "\n"), Is.EqualTo ("flowed\n"), "Flowed");
			Assert.That (outer.GetTextBody (TextFormat.RichText).Replace ("\r\n", "\n"), Is.EqualTo ("rtf\n"), "RichText");
			Assert.That (outer.GetTextBody (TextFormat.Html).Replace ("\r\n", "\n"), Is.EqualTo ("html\n"), "Html");
			Assert.That (outer.GetTextBody (TextFormat.Enriched), Is.Null, "Enriched");
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

			Assert.That (alternative.TextBody.Replace ("\r\n", "\n"), Is.EqualTo ("plain\n"), "TextBody");
			//Assert.That (alternative.HtmlBody.Replace ("\r\n", "\n"), Is.EqualTo ("html\n"), "HtmlBody");

			mixed.Insert (1, flowed);

			// Note: Only the text/plain part will be found because Multipart.TryGetValue() will only look at the very first text part.
			Assert.That (alternative.GetTextBody (TextFormat.Plain).Replace ("\r\n", "\n"), Is.EqualTo ("plain\n"), "Plain");
			Assert.That (alternative.GetTextBody (TextFormat.Flowed), Is.Null, "Flowed");
			Assert.That (alternative.GetTextBody (TextFormat.RichText), Is.Null, "RichText");
			Assert.That (alternative.GetTextBody (TextFormat.Html), Is.Null, "Html");
			Assert.That (alternative.GetTextBody (TextFormat.Enriched), Is.Null, "Enriched");
		}
	}
}
