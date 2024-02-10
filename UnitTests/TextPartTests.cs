//
// TextPartTests.cs
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

namespace UnitTests {
	[TestFixture]
	public class TextPartTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var text = new TextPart (TextFormat.Plain);

			Assert.Throws<ArgumentNullException> (() => new TextPart ("plain", (object[]) null));
			Assert.Throws<ArgumentException> (() => new TextPart ("plain", Encoding.UTF8, "blah blah blah", Encoding.UTF8));
			Assert.Throws<ArgumentException> (() => new TextPart ("plain", Encoding.UTF8, "blah blah blah", "blah blah"));
			Assert.Throws<ArgumentException> (() => new TextPart ("plain", 5));
			Assert.Throws<ArgumentOutOfRangeException> (() => new TextPart ((TextFormat) 500));

			Assert.Throws<ArgumentNullException> (() => text.Accept (null));
			Assert.Throws<ArgumentNullException> (() => text.GetText ((string) null));
			Assert.Throws<ArgumentNullException> (() => text.GetText ((Encoding) null));
			Assert.Throws<ArgumentNullException> (() => text.SetText ((string) null, "text"));
			Assert.Throws<ArgumentNullException> (() => text.SetText ((Encoding) null, "text"));
			Assert.Throws<ArgumentNullException> (() => text.SetText ("iso-8859-1", null));
			Assert.Throws<ArgumentNullException> (() => text.SetText (Encoding.UTF8, null));
		}

		[Test]
		public void TestFormat ()
		{
			TextPart text;

			text = new TextPart (TextFormat.Html);
			Assert.That (text.IsHtml, Is.True, "IsHtml");
			Assert.That (text.IsPlain, Is.False, "IsPlain");
			Assert.That (text.IsFlowed, Is.False, "IsFlowed");
			Assert.That (text.IsEnriched, Is.False, "IsEnriched");
			Assert.That (text.IsRichText, Is.False, "IsRichText");
			Assert.That (text.Format, Is.EqualTo (TextFormat.Html), "Format");
			Assert.That (text.IsFormat (TextFormat.Html), Is.True, "IsFormat");

			text = new TextPart (TextFormat.Plain);
			Assert.That (text.IsHtml, Is.False, "IsHtml");
			Assert.That (text.IsPlain, Is.True, "IsPlain");
			Assert.That (text.IsFlowed, Is.False, "IsFlowed");
			Assert.That (text.IsEnriched, Is.False, "IsEnriched");
			Assert.That (text.IsRichText, Is.False, "IsRichText");
			Assert.That (text.Format, Is.EqualTo (TextFormat.Plain), "Format");
			Assert.That (text.IsFormat (TextFormat.Plain), Is.True, "IsFormat");

			text = new TextPart (TextFormat.Flowed);
			Assert.That (text.IsHtml, Is.False, "IsHtml");
			Assert.That (text.IsPlain, Is.True, "IsPlain"); // special: Flowed is both Plain *and* Flowed
			Assert.That (text.IsFlowed, Is.True, "IsFlowed");
			Assert.That (text.IsEnriched, Is.False, "IsEnriched");
			Assert.That (text.IsRichText, Is.False, "IsRichText");
			Assert.That (text.Format, Is.EqualTo (TextFormat.Flowed), "Format");
			Assert.That (text.IsFormat (TextFormat.Plain), Is.True, "IsFormat"); // special: Flowed is both Plain *and* Flowed
			Assert.That (text.IsFormat (TextFormat.Flowed), Is.True, "IsFormat");

			text = new TextPart (TextFormat.RichText);
			Assert.That (text.IsHtml, Is.False, "IsHtml");
			Assert.That (text.IsPlain, Is.False, "IsPlain");
			Assert.That (text.IsFlowed, Is.False, "IsFlowed");
			Assert.That (text.IsEnriched, Is.False, "IsEnriched");
			Assert.That (text.IsRichText, Is.True, "IsRichText");
			Assert.That (text.Format, Is.EqualTo (TextFormat.RichText), "Format");
			Assert.That (text.IsFormat (TextFormat.RichText), Is.True, "IsFormat");

			text = new TextPart (new ContentType ("application", "rtf"));
			Assert.That (text.IsHtml, Is.False, "IsHtml");
			Assert.That (text.IsPlain, Is.False, "IsPlain");
			Assert.That (text.IsFlowed, Is.False, "IsFlowed");
			Assert.That (text.IsEnriched, Is.False, "IsEnriched");
			Assert.That (text.IsRichText, Is.True, "IsRichText");
			Assert.That (text.Format, Is.EqualTo (TextFormat.RichText), "Format");
			Assert.That (text.IsFormat (TextFormat.RichText), Is.True, "IsFormat");
			Assert.That (text.IsFormat (TextFormat.CompressedRichText), Is.False, "CompressedRichText");

			text = new TextPart (TextFormat.Enriched);
			Assert.That (text.IsHtml, Is.False, "IsHtml");
			Assert.That (text.IsPlain, Is.False, "IsPlain");
			Assert.That (text.IsFlowed, Is.False, "IsFlowed");
			Assert.That (text.IsEnriched, Is.True, "IsEnriched");
			Assert.That (text.IsRichText, Is.False, "IsRichText");
			Assert.That (text.Format, Is.EqualTo (TextFormat.Enriched), "Format");
			Assert.That (text.IsFormat (TextFormat.Enriched), Is.True, "IsFormat");

			text = new TextPart ("richtext");
			Assert.That (text.IsHtml, Is.False, "IsHtml");
			Assert.That (text.IsPlain, Is.False, "IsPlain");
			Assert.That (text.IsFlowed, Is.False, "IsFlowed");
			Assert.That (text.IsEnriched, Is.True, "IsEnriched");
			Assert.That (text.IsRichText, Is.False, "IsRichText");
			Assert.That (text.Format, Is.EqualTo (TextFormat.Enriched), "Format");
			Assert.That (text.IsFormat (TextFormat.Enriched), Is.True, "IsFormat");
		}

		[Test]
		public void TestGetText ()
		{
			const string text = "This is some Låtín1 text.";

			var encoding = Encoding.GetEncoding ("iso-8859-1");
			var part = new TextPart ("plain");

			part.SetText ("iso-8859-1", text);

			Assert.That (part.GetText ("iso-8859-1"), Is.EqualTo (text), "charset");
			Assert.That (part.GetText (encoding), Is.EqualTo (text), "encoding");
		}

		[Test]
		public void TestInvalidCharset ()
		{
			const string text = "This is some Låtín1 text.";

			var latin1 = Encoding.GetEncoding ("iso-8859-1");
			var part = new TextPart ("plain");

			part.SetText ("iso-8859-1", text);
			part.ContentType.Charset = "flubber";

			Assert.That (part.Text, Is.EqualTo (text));

			var actual = part.GetText (out Encoding encoding);

			Assert.That (actual, Is.EqualTo (text), "GetText(out Encoding)");
			Assert.That (encoding.CodePage, Is.EqualTo (latin1.CodePage), "Encoding");
		}

		[Test]
		public void TestNullContentIsAscii ()
		{
			var part = new TextPart ("plain");

			Assert.That (part.Text, Is.EqualTo (string.Empty), "Text");
			Assert.That (part.GetText (Encoding.ASCII), Is.EqualTo (string.Empty), "GetText");

			var actual = part.GetText (out Encoding encoding);

			Assert.That (actual, Is.EqualTo (string.Empty), "GetText(out Encoding)");
			Assert.That (encoding.CodePage, Is.EqualTo (Encoding.ASCII.CodePage), "Encoding");
		}

		[Test]
		public void TestLatin1 ()
		{
			const string text = "This is some Låtín1 text.";

			var latin1 = Encoding.GetEncoding ("iso-8859-1");
			var memory = new MemoryStream ();
			var buffer = latin1.GetBytes (text);
			memory.Write (buffer, 0, buffer.Length);
			memory.Position = 0;

			var part = new TextPart ("plain") { Content = new MimeContent (memory) };

			Assert.That (part.Text, Is.EqualTo (text));

			var actual = part.GetText (out Encoding encoding);

			Assert.That (actual, Is.EqualTo (text), "GetText(out Encoding)");
			Assert.That (encoding.CodePage, Is.EqualTo (latin1.CodePage), "Encoding");
		}

		[Test]
		public void TestUTF16BE ()
		{
			const string text = "This is some UTF-16BE text.\r\nThis is line #2.";
			var expected = text.Replace ("\r\n", Environment.NewLine);

			var memory = new MemoryStream ();
			memory.WriteByte (0xfe);
			memory.WriteByte (0xff);

			var buffer = Encoding.BigEndianUnicode.GetBytes (text);
			memory.Write (buffer, 0, buffer.Length);
			memory.Position = 0;

			var part = new TextPart ("plain") { Content = new MimeContent (memory) };

			Assert.That (part.Text.Substring (1), Is.EqualTo (expected));

			var actual = part.GetText (out Encoding encoding);

			Assert.That (actual.Substring (1), Is.EqualTo (expected), "GetText(out Encoding)");
			Assert.That (encoding.CodePage, Is.EqualTo (Encoding.BigEndianUnicode.CodePage), "Encoding");
		}

		[Test]
		public void TestUTF16LE ()
		{
			const string text = "This is some UTF-16LE text.\r\nThis is line #2.";
			var expected = text.Replace ("\r\n", Environment.NewLine);

			var memory = new MemoryStream ();
			memory.WriteByte (0xff);
			memory.WriteByte (0xfe);

			var buffer = Encoding.Unicode.GetBytes (text);
			memory.Write (buffer, 0, buffer.Length);
			memory.Position = 0;

			var part = new TextPart ("plain") { Content = new MimeContent (memory) };

			Assert.That (part.Text.Substring (1), Is.EqualTo (expected));

			var actual = part.GetText (out Encoding encoding);

			Assert.That (actual.Substring (1), Is.EqualTo (expected), "GetText(out Encoding)");
			Assert.That (encoding.CodePage, Is.EqualTo (Encoding.Unicode.CodePage), "Encoding");
		}

		[Test]
		public void TestTryDetectEncodingNoContent ()
		{
			var part = new TextPart (TextFormat.Html);
			TextEncodingConfidence confidence;
			Encoding encoding;

			Assert.That (part.TryDetectEncoding (out encoding, out confidence), Is.True);
			Assert.That (confidence, Is.EqualTo (TextEncodingConfidence.Irrelevant));
			Assert.That (encoding.WebName, Is.EqualTo ("us-ascii"));
		}

		[Test]
		public void TestTryDetectEncodingByteOrderMarkUTF8 ()
		{
			const string html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=iso-8859-1\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream ()) {
				stream.WriteByte (0xef);
				stream.WriteByte (0xbb);
				stream.WriteByte (0xbf);

				var buffer = Encoding.ASCII.GetBytes (html);
				stream.Write (buffer, 0, buffer.Length);
				stream.Position = 0;

				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};
				TextEncodingConfidence confidence;
				Encoding encoding;

				Assert.That (part.TryDetectEncoding (out encoding, out confidence), Is.True);
				Assert.That (confidence, Is.EqualTo (TextEncodingConfidence.Certain));
				Assert.That (encoding.WebName, Is.EqualTo ("utf-8"));
			}
		}

		[Test]
		public void TestTryDetectEncodingByteOrderMarkUTF16BE ()
		{
			const string html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=iso-8859-1\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream ()) {
				stream.WriteByte (0xfe);
				stream.WriteByte (0xff);

				var buffer = Encoding.BigEndianUnicode.GetBytes (html);
				stream.Write (buffer, 0, buffer.Length);
				stream.Position = 0;

				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};
				TextEncodingConfidence confidence;
				Encoding encoding;

				Assert.That (part.TryDetectEncoding (out encoding, out confidence), Is.True);
				Assert.That (confidence, Is.EqualTo (TextEncodingConfidence.Certain));
				Assert.That (encoding.WebName, Is.EqualTo ("utf-16BE"));
			}
		}

		[Test]
		public void TestTryDetectEncodingByteOrderMarkUTF16LE ()
		{
			const string html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=iso-8859-1\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream ()) {
				stream.WriteByte (0xff);
				stream.WriteByte (0xfe);

				var buffer = Encoding.Unicode.GetBytes (html);
				stream.Write (buffer, 0, buffer.Length);
				stream.Position = 0;

				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};
				TextEncodingConfidence confidence;
				Encoding encoding;

				Assert.That (part.TryDetectEncoding (out encoding, out confidence), Is.True);
				Assert.That (confidence, Is.EqualTo (TextEncodingConfidence.Certain));
				Assert.That (encoding.WebName, Is.EqualTo ("utf-16"));
			}
		}

		[Test]
		public void TestTryDetectHtmlEncoding ()
		{
			const string html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=euc-kr\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};
				TextEncodingConfidence confidence;
				Encoding encoding;

				Assert.That (part.TryDetectEncoding (out encoding, out confidence), Is.True);
				Assert.That (confidence, Is.EqualTo (TextEncodingConfidence.Tentative));
				Assert.That (encoding.WebName, Is.EqualTo ("euc-kr"));
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingInvalidCharset ()
		{
			const string html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=x-unknown\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};
				TextEncodingConfidence confidence;
				Encoding encoding;

				Assert.That (part.TryDetectEncoding (out encoding, out confidence), Is.False);
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingXUserDefined ()
		{
			const string html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=x-user-defined\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};
				TextEncodingConfidence confidence;
				Encoding encoding;

				Assert.That (part.TryDetectEncoding (out encoding, out confidence), Is.True);
				Assert.That (confidence, Is.EqualTo (TextEncodingConfidence.Tentative));
				Assert.That (encoding.WebName.ToLowerInvariant (), Is.EqualTo ("windows-1252"));
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingCharsetAttribute ()
		{
			const string html = "<html><head><meta charset=\"x-user-defined\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};
				TextEncodingConfidence confidence;
				Encoding encoding;

				Assert.That (part.TryDetectEncoding (out encoding, out confidence), Is.True);
				Assert.That (confidence, Is.EqualTo (TextEncodingConfidence.Tentative));
				Assert.That (encoding.WebName.ToLowerInvariant (), Is.EqualTo ("windows-1252"));
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingHttpEquivAndCharsetAttributes ()
		{
			const string html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=iso-8859-1\" charset=\"windows-1252\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};
				TextEncodingConfidence confidence;
				Encoding encoding;

				Assert.That (part.TryDetectEncoding (out encoding, out confidence), Is.True);
				Assert.That (confidence, Is.EqualTo (TextEncodingConfidence.Tentative));
				Assert.That (encoding.WebName.ToLowerInvariant (), Is.EqualTo ("windows-1252"));
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingHttpEquivAndCharsetAttributesReversed ()
		{
			const string html = "<html><head><meta charset=\"windows-1252\" http-equiv=\"Content-Type\" content=\"text/html; charset=iso-8859-1\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};
				TextEncodingConfidence confidence;
				Encoding encoding;

				Assert.That (part.TryDetectEncoding (out encoding, out confidence), Is.True);
				Assert.That (confidence, Is.EqualTo (TextEncodingConfidence.Tentative));
				Assert.That (encoding.WebName.ToLowerInvariant (), Is.EqualTo ("windows-1252"));
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingNoCharsetParameter ()
		{
			const string html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};

				Assert.That (part.TryDetectEncoding (out _, out _), Is.False);
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingInvalidContentType ()
		{
			const string html = "<html><head><meta http-equiv=\"Content-Type\" content=\"this is invalid\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};

				Assert.That (part.TryDetectEncoding (out _, out _), Is.False);
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingEmptyContentAttribute ()
		{
			const string html = "<html><head><meta http-equiv=\"Content-Type\" content /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};

				Assert.That (part.TryDetectEncoding (out _, out _), Is.False);
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingNoContentAttribute ()
		{
			const string html = "<html><head><meta http-equiv=\"Content-Type\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};

				Assert.That (part.TryDetectEncoding (out _, out _), Is.False);
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingEmptyHttpEquivNotContentType ()
		{
			const string html = "<html><head><meta http-equiv=\"Content-Transfer-Encoding\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};

				Assert.That (part.TryDetectEncoding (out _, out _), Is.False);
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingEmptyHttpEquivAttribute ()
		{
			const string html = "<html><head><meta http-equiv /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};

				Assert.That (part.TryDetectEncoding (out _, out _), Is.False);
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingMetaNotHttpEquiv ()
		{
			const string html = "<html><head><meta data=\"metadata\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};

				Assert.That (part.TryDetectEncoding (out _, out _), Is.False);
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingNoMetaTags ()
		{
			const string html = "<html><head></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};

				Assert.That (part.TryDetectEncoding (out _, out _), Is.False);
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingEmptyHtmlTag ()
		{
			const string html = "<html /><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=euc-kr\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};

				Assert.That (part.TryDetectEncoding (out _, out _), Is.False);
			}
		}

		[Test]
		public void TestTryDetectHtmlEncodingEmptyHeadTag ()
		{
			const string html = "<html><head /><meta http-equiv=\"Content-Type\" content=\"text/html; charset=euc-kr\" /></head><body><p>Hello, world!</p></body></html>";

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (html), false)) {
				var part = new TextPart (TextFormat.Html) {
					Content = new MimeContent (stream)
				};

				Assert.That (part.TryDetectEncoding (out _, out _), Is.False);
			}
		}
	}
}
