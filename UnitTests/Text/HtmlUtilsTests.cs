//
// HtmlUtilsTests.cs
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
	public class HtmlUtilsTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var writer = new StringWriter ();
			const string text = "text";

			// HtmlAttributeEncode
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode ((string) null));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (text, 'x'));

			//Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode ((ReadOnlySpan<char>) null));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (text.AsSpan (), 'x'));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (null, text));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (writer, (string) null));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (writer, text, 'x'));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (null, text.AsSpan ()));
			//Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (writer, (ReadOnlySpan<char>) null));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (writer, text.AsSpan (), 'x'));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode ((string) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (text, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (text, 0, text.Length + 1));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (text, 0, text.Length, 'x'));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode ((char[]) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (text.ToCharArray (), -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (text.ToCharArray (), 0, text.Length + 1));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (text.ToCharArray (), 0, text.Length, 'x'));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (null, text, 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (writer, (string) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (writer, text, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (writer, text, 0, text.Length + 1));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (writer, text, 0, text.Length, 'x'));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (null, text.ToCharArray (), 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (writer, (char[]) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (writer, text.ToCharArray (), -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (writer, text.ToCharArray (), 0, text.Length + 1));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (writer, text.ToCharArray (), 0, text.Length, 'x'));

			// HtmlEncode
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode ((string) null));
			//Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode ((ReadOnlySpan<char>) null));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (null, text));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (writer, (string) null));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (null, text.AsSpan ()));
			//Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (writer, (ReadOnlySpan<char>) null));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode ((string) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (text, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (text, 0, text.Length + 1));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode ((char[]) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (text.ToCharArray (), -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (text.ToCharArray (), 0, text.Length + 1));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (null, text, 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (writer, (string) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (writer, text, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (writer, text, 0, text.Length + 1));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (null, text.ToCharArray (), 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (writer, (char[]) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (writer, text.ToCharArray (), -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlEncode (writer, text.ToCharArray (), 0, text.Length + 1));

			// HtmlDecode
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlDecode (null));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlDecode (null, text));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlDecode (writer, null));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlDecode (null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlDecode (text, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlDecode (text, 0, text.Length + 1));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlDecode (null, text, 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlDecode (writer, null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlDecode (writer, text, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlDecode (writer, text, 0, text.Length + 1));
		}

		static void AssertHtmlAttributeEncode (string text, string expected)
		{
			string encoded;

			encoded = HtmlUtils.HtmlAttributeEncode (text);
			Assert.That (encoded, Is.EqualTo (expected), "HtmlAttributeEncode(string)");

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlAttributeEncode (writer, text);
				encoded = writer.ToString ();
				Assert.That (encoded, Is.EqualTo (expected), "HtmlAttributeEncode(TextWriter,string)");
			}

			encoded = HtmlUtils.HtmlAttributeEncode (text, 0, text.Length);
			Assert.That (encoded, Is.EqualTo (expected), "HtmlAttributeEncode(string,int,int)");

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlAttributeEncode (writer, text, 0, text.Length);
				encoded = writer.ToString ();
				Assert.That (encoded, Is.EqualTo (expected), "HtmlAttributeEncode(TextWriter,string,int,int)");
			}

			encoded = HtmlUtils.HtmlAttributeEncode (text.ToCharArray (), 0, text.Length);
			Assert.That (encoded, Is.EqualTo (expected), "HtmlAttributeEncode(char[],int,int)");

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlAttributeEncode (writer, text.ToCharArray (), 0, text.Length);
				encoded = writer.ToString ();
				Assert.That (encoded, Is.EqualTo (expected), "HtmlAttributeEncode(TextWriter,char[],int,int)");
			}
		}

		static void AssertHtmlEncode (string text, string expected, bool testDecode)
		{
			string encoded, decoded;

			encoded = HtmlUtils.HtmlEncode (text);
			Assert.That (encoded, Is.EqualTo (expected), "HtmlEncode(string)");

			encoded = HtmlUtils.HtmlEncode (text, 0, text.Length);
			Assert.That (encoded, Is.EqualTo (expected), "HtmlEncode(string,int,int)");

			encoded = HtmlUtils.HtmlEncode (text.ToCharArray (), 0, text.Length);
			Assert.That (encoded, Is.EqualTo (expected), "HtmlEncode(char[],int,int)");

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlEncode (writer, text);
				encoded = writer.ToString ();
				Assert.That (encoded, Is.EqualTo (expected), "HtmlEncode(TextWriter,string)");
			}

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlEncode (writer, text, 0, text.Length);
				encoded = writer.ToString ();
				Assert.That (encoded, Is.EqualTo (expected), "HtmlEncode(TextWriter,string,int,int)");
			}

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlEncode (writer, text.ToCharArray (), 0, text.Length);
				encoded = writer.ToString ();
				Assert.That (encoded, Is.EqualTo (expected), "HtmlEncode(TextWriter,char[],int,int)");
			}

			if (testDecode) {
				decoded = HtmlUtils.HtmlDecode (encoded);
				Assert.That (decoded, Is.EqualTo (text), "HtmlDecode(string)");

				decoded = HtmlUtils.HtmlDecode (encoded, 0, encoded.Length);
				Assert.That (decoded, Is.EqualTo (text), "HtmlDecode(string,int,int)");

				using (var writer = new StringWriter ()) {
					HtmlUtils.HtmlDecode (writer, encoded);
					decoded = writer.ToString ();
					Assert.That (decoded, Is.EqualTo (text), "HtmlDecode(TextWriter,string)");
				}

				using (var writer = new StringWriter ()) {
					HtmlUtils.HtmlDecode (writer, encoded, 0, encoded.Length);
					decoded = writer.ToString ();
					Assert.That (decoded, Is.EqualTo (text), "HtmlDecode(TextWriter,string,int,int)");
				}
			}
		}

		[Test]
		public void TestEncode ()
		{
			const string attributeValue = "\"if (showJapaneseText &amp;&amp; x &gt; 0 &amp;&amp; x &lt;= 1)\ttext = '&#29378;&#12387;&#12383;&#12371;&#12398;&#19990;&#12391;&#29378;&#12358;&#12394;&#12425;&#27671;&#12399;&#30906;&#12363;&#12384;&#12290;';\"";
			const string encoded = "if (showJapaneseText &amp;&amp; x &gt; 0 &amp;&amp; x &lt;= 1)\ttext = &#39;&#29378;&#12387;&#12383;&#12371;&#12398;&#19990;&#12391;&#29378;&#12358;&#12394;&#12425;&#27671;&#12399;&#30906;&#12363;&#12384;&#12290;&#39;;";
			const string text = "if (showJapaneseText && x > 0 && x <= 1)\ttext = '狂ったこの世で狂うなら気は確かだ。';";

			AssertHtmlAttributeEncode (text, attributeValue);
			AssertHtmlEncode (text, encoded, true);
		}

		[Test]
		public void TestEncodeSurrogatePairs ()
		{
			const string attributeValue = "\"This emoji (&#128561;) contains a surrogate pair. And this next one is truncated: &#55357;\"";
			const string encoded = "This emoji (&#128561;) contains a surrogate pair. And this next one is truncated: &#55357;";
			var emoji = Encoding.UTF8.GetString (Convert.FromBase64String ("8J+YsQ=="));
			var text = $"This emoji ({emoji}) contains a surrogate pair. And this next one is truncated: {emoji[0]}";

			AssertHtmlAttributeEncode (text, attributeValue);
			AssertHtmlEncode (text, encoded, false);
		}

		[Test]
		public void TestEncodeIllegalControlCharacters ()
		{
			const string attributeValue = "\"This contains some embedded control sequences ()\"";
			const string encoded = "This contains some embedded control sequences ()";
			var text = "This contains some embedded control sequences (\x19\x80\x9F)";

			AssertHtmlAttributeEncode (text, attributeValue);
			AssertHtmlEncode (text, encoded, false);
		}

		[Test]
		public void TestHtmlDecode ()
		{
			const string encoded = "&lt;&pound;&euro;&cent;&yen;&nbsp;&copy;&reg;&gt;";
			const string expected = "<£€¢¥\u00a0©®>";

			var decoded = HtmlUtils.HtmlDecode (encoded);

			Assert.That (decoded, Is.EqualTo (expected));
		}

		[Test]
		public void TestHtmlNamespaces ()
		{
			string nullspace = null;

			Assert.Throws<ArgumentNullException> (() => nullspace.ToHtmlNamespace ());

			Assert.That ("does not exist".ToHtmlNamespace (), Is.EqualTo (HtmlNamespace.Html));

			Assert.Throws<ArgumentOutOfRangeException> (() => ((HtmlNamespace) 500).ToNamespaceUrl ());

			foreach (HtmlNamespace ns in Enum.GetValues (typeof (HtmlNamespace))) {
				var value = ns.ToNamespaceUrl ().ToHtmlNamespace ();

				Assert.That (value, Is.EqualTo (ns));
			}
		}

		[Test]
		public void TestIsValidTokenName ()
		{
			Assert.That (HtmlUtils.IsValidTokenName (string.Empty), Is.False, "string.Empty");
		}
	}
}
