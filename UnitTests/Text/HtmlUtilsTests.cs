//
// HtmlUtilsTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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

using NUnit.Framework;

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
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (null));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (text, 'x'));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (null, text));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (writer, null));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (writer, text, 'x'));

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

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (null, text.ToCharArray(), 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlAttributeEncode (writer, (char[]) null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (writer, text.ToCharArray (), -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => HtmlUtils.HtmlAttributeEncode (writer, text.ToCharArray (), 0, text.Length + 1));
			Assert.Throws<ArgumentException> (() => HtmlUtils.HtmlAttributeEncode (writer, text.ToCharArray (), 0, text.Length, 'x'));

			// HtmlEncode
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (null));

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (null, text));
			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (writer, null));

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

			Assert.Throws<ArgumentNullException> (() => HtmlUtils.HtmlEncode (null, text.ToCharArray(), 0, text.Length));
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
			Assert.AreEqual (expected, encoded, "HtmlAttributeEncode(string)");

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlAttributeEncode (writer, text);
				encoded = writer.ToString ();
				Assert.AreEqual (expected, encoded, "HtmlAttributeEncode(TextWriter,string)");
			}

			encoded = HtmlUtils.HtmlAttributeEncode (text, 0, text.Length);
			Assert.AreEqual (expected, encoded, "HtmlAttributeEncode(string,int,int)");

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlAttributeEncode (writer, text, 0, text.Length);
				encoded = writer.ToString ();
				Assert.AreEqual (expected, encoded, "HtmlAttributeEncode(TextWriter,string,int,int)");
			}

			encoded = HtmlUtils.HtmlAttributeEncode (text.ToCharArray (), 0, text.Length);
			Assert.AreEqual (expected, encoded, "HtmlAttributeEncode(char[],int,int)");

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlAttributeEncode (writer, text.ToCharArray (), 0, text.Length);
				encoded = writer.ToString ();
				Assert.AreEqual (expected, encoded, "HtmlAttributeEncode(TextWriter,char[],int,int)");
			}
		}

		static void AssertHtmlEncode (string text, string expected)
		{
			string encoded, decoded;

			encoded = HtmlUtils.HtmlEncode (text);
			Assert.AreEqual (expected, encoded, "HtmlEncode(string)");

			encoded = HtmlUtils.HtmlEncode (text, 0, text.Length);
			Assert.AreEqual (expected, encoded, "HtmlEncode(string,int,int)");

			encoded = HtmlUtils.HtmlEncode (text.ToCharArray (), 0, text.Length);
			Assert.AreEqual (expected, encoded, "HtmlEncode(char[],int,int)");

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlEncode (writer, text);
				encoded = writer.ToString ();
				Assert.AreEqual (expected, encoded, "HtmlEncode(TextWriter,string)");
			}

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlEncode (writer, text, 0, text.Length);
				encoded = writer.ToString ();
				Assert.AreEqual (expected, encoded, "HtmlEncode(TextWriter,string,int,int)");
			}

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlEncode (writer, text.ToCharArray (), 0, text.Length);
				encoded = writer.ToString ();
				Assert.AreEqual (expected, encoded, "HtmlEncode(TextWriter,char[],int,int)");
			}

			decoded = HtmlUtils.HtmlDecode (encoded);
			Assert.AreEqual (text, decoded, "HtmlDecode(string)");

			decoded = HtmlUtils.HtmlDecode (encoded, 0, encoded.Length);
			Assert.AreEqual (text, decoded, "HtmlDecode(string,int,int)");

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlDecode (writer, encoded);
				decoded = writer.ToString ();
				Assert.AreEqual (text, decoded, "HtmlDecode(TextWriter,string)");
			}

			using (var writer = new StringWriter ()) {
				HtmlUtils.HtmlDecode (writer, encoded, 0, encoded.Length);
				decoded = writer.ToString ();
				Assert.AreEqual (text, decoded, "HtmlDecode(TextWriter,string,int,int)");
			}
		}

		[Test]
		public void TestEncode ()
		{
			const string attributeValue = "\"if (showJapaneseText &amp;&amp; x &lt;= 1)\ttext = '&#29378;&#12387;&#12383;&#12371;&#12398;&#19990;&#12391;&#29378;&#12358;&#12394;&#12425;&#27671;&#12399;&#30906;&#12363;&#12384;&#12290;';\"";
			const string encoded = "if (showJapaneseText &amp;&amp; x &lt;= 1)\ttext = &#39;&#29378;&#12387;&#12383;&#12371;&#12398;&#19990;&#12391;&#29378;&#12358;&#12394;&#12425;&#27671;&#12399;&#30906;&#12363;&#12384;&#12290;&#39;;";
			const string text = "if (showJapaneseText && x <= 1)\ttext = '狂ったこの世で狂うなら気は確かだ。';";

			AssertHtmlAttributeEncode (text, attributeValue);
			AssertHtmlEncode (text, encoded);
		}

		[Test]
		public void TestHtmlDecode ()
		{
			const string encoded = "&lt;&pound;&euro;&cent;&yen;&nbsp;&copy;&reg;&gt;";
			const string expected = "<£€¢¥\u00a0©®>";

			var decoded = HtmlUtils.HtmlDecode (encoded);

			Assert.AreEqual (expected, decoded);
		}

		[Test]
		public void TestHtmlNamespaces ()
		{
			string nullspace = null;

			Assert.Throws<ArgumentNullException> (() => nullspace.ToHtmlNamespace ());

			Assert.AreEqual (HtmlNamespace.Html, "does not exist".ToHtmlNamespace ());

			Assert.Throws<ArgumentOutOfRangeException> (() => ((HtmlNamespace) 500).ToNamespaceUrl ());

			foreach (HtmlNamespace ns in Enum.GetValues (typeof (HtmlNamespace))) {
				var value = ns.ToNamespaceUrl ().ToHtmlNamespace ();

				Assert.AreEqual (ns, value);
			}
		}
	}
}
