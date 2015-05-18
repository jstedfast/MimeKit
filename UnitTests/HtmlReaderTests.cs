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
			using (var textReader = File.OpenText ("../../TestData/html/xamarin3.html")) {
				using (var htmlReader = new HtmlReader (textReader)) {
					HtmlToken token;

					Assert.AreEqual (HtmlReaderState.Initial, htmlReader.State);

					while (htmlReader.ReadNextToken (out token)) {
						Console.Write ("{0}: ", token.Kind);
						switch (token.Kind) {
						case HtmlTokenKind.Text:
							var text = (HtmlTokenText) token;
							var buf = new char[4096];
							int nread;

							while ((nread = text.Read (buf, 0, buf.Length)) > 0) {
								for (int i = 0; i < nread; i++) {
									switch (buf[i]) {
									case '\t': Console.Write ("\\t"); break;
									case '\r': Console.Write ("\\r"); break;
									case '\n': Console.Write ("\\n"); break;
									default: Console.Write (buf[i]); break;
									}
								}
							}
							Console.WriteLine ();
							break;
						case HtmlTokenKind.EmptyElementTag:
						case HtmlTokenKind.StartTag:
						case HtmlTokenKind.EndTag:
							var tag = (HtmlTokenTag) token;
							Console.Write (tag.TagName);
							var attributes = tag.AttributeReader;
							while (attributes.ReadNext ()) {
								if (tag.Kind == HtmlTokenKind.EndTag)
									Assert.Fail ("HTML end tags should not have attributes!");

								if (attributes.HasValue)
									Console.Write ("; {0}={1}", attributes.Name, MimeUtils.Quote (attributes.Value));
								else
									Console.Write ("; {0}", attributes.Name);
							}
							Console.WriteLine ();
							break;
						case HtmlTokenKind.Comment:
							var comment = (HtmlTokenComment) token;
							Console.WriteLine (comment.Comment);
							break;
						case HtmlTokenKind.DocType:
							var doctype = (HtmlTokenDocType) token;
							Console.WriteLine (doctype.DocType);
							break;
						default:
							Assert.Fail ("Unhandled token type: {0}", token.Kind);
							break;
						}
					}

					Assert.AreEqual (HtmlReaderState.EndOfFile, htmlReader.State);
				}
			}

			Assert.Fail ();
		}
	}
}
