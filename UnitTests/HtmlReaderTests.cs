//
// HtmlReaderTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

using MimeKit.Text;
using MimeKit.Utils;

using NUnit.Framework;

namespace UnitTests {
	[TestFixture]
	public class HtmlReaderTests
	{
		[Test]
		public void TestXamarin3SampleHtml ()
		{
			var expected = File.ReadAllText ("../../TestData/html/xamarin3.tokens");
			var actual = new StringBuilder ();

			using (var textReader = File.OpenText ("../../TestData/html/xamarin3.html")) {
				using (var htmlReader = new HtmlReader (textReader)) {
					HtmlToken token;

					Assert.AreEqual (HtmlReaderState.Initial, htmlReader.State);

					while (htmlReader.ReadNextToken (out token)) {
						actual.AppendFormat ("{0}: ", token.Kind);
						switch (token.Kind) {
						case HtmlTokenKind.Text:
							var text = (HtmlTokenText) token;
							var buf = new char[4096];
							int nread;

							while ((nread = text.Read (buf, 0, buf.Length)) > 0) {
								for (int i = 0; i < nread; i++) {
									switch (buf[i]) {
									case '\t': actual.Append ("\\t"); break;
									case '\r': actual.Append ("\\r"); break;
									case '\n': actual.Append ("\\n"); break;
									default: actual.Append (buf[i]); break;
									}
								}
							}
							actual.AppendLine ();
							break;
						case HtmlTokenKind.EmptyElementTag:
						case HtmlTokenKind.StartTag:
						case HtmlTokenKind.EndTag:
							var tag = (HtmlTokenTag) token;
							actual.Append (tag.TagName);
							var attributes = tag.AttributeReader;
							while (attributes.ReadNext ()) {
								if (tag.Kind == HtmlTokenKind.EndTag)
									Assert.Fail ("HTML end tags should not have attributes!");

								if (attributes.HasValue)
									actual.AppendFormat ("; {0}={1}", attributes.Name, MimeUtils.Quote (attributes.Value));
								else
									actual.AppendFormat ("; {0}", attributes.Name);
							}
							actual.AppendLine ();
							break;
						case HtmlTokenKind.Comment:
							var comment = (HtmlTokenComment) token;
							actual.AppendLine (comment.Comment);
							break;
						case HtmlTokenKind.DocType:
							var doctype = (HtmlTokenDocType) token;
							actual.AppendLine (doctype.DocType);
							break;
						default:
							Assert.Fail ("Unhandled token type: {0}", token.Kind);
							break;
						}
					}

					Assert.AreEqual (HtmlReaderState.EndOfFile, htmlReader.State);
				}
			}

			Assert.AreEqual (expected, actual.ToString (), "The token stream does not match the expected tokens.");
		}

		[Test]
		public void TestHtmlDecode ()
		{
			const string encoded = "&lt;&pound;&euro;&cent;&yen;&nbsp;&copy;&reg;&gt;";
			const string expected = "<£€¢¥\u00a0©®>";

			var decoded = HtmlUtils.HtmlDecode (encoded, 0, encoded.Length);

			Assert.AreEqual (expected, decoded);
		}
	}
}
