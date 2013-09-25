//
// MimeParserTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
using NUnit.Framework;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class MimeParserTests
	{
		[Test]
		public void TestSimpleMbox ()
		{
			using (var stream = File.OpenRead ("../../TestData/mbox/simple.mbox.txt")) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				while (!parser.IsEndOfStream) {
					var message = parser.ParseMessage ();
					Multipart multipart;
					MimeEntity entity;

					Assert.IsInstanceOfType (typeof (Multipart), message.Body);
					multipart = (Multipart) message.Body;
					Assert.AreEqual (1, multipart.Count);
					entity = multipart[0];

					Assert.IsInstanceOfType (typeof (Multipart), entity);
					multipart = (Multipart) entity;
					Assert.AreEqual (1, multipart.Count);
					entity = multipart[0];

					Assert.IsInstanceOfType (typeof (Multipart), entity);
					multipart = (Multipart) entity;
					Assert.AreEqual (1, multipart.Count);
					entity = multipart[0];

					Assert.IsInstanceOfType (typeof (TextPart), entity);

					using (var memory = new MemoryStream ()) {
						entity.WriteTo (memory);

						var text = Encoding.ASCII.GetString (memory.ToArray ());
						Assert.IsTrue (text.StartsWith ("Content-Type: text/plain\n\n"), "Headers are not properly terminated.");
					}
				}
			}
		}

		static void DumpMimeTree (StringBuilder builder, MimeEntity entity, int depth)
		{
			if (depth > 0)
				builder.Append (new string (' ', depth * 3));

			builder.AppendFormat ("Content-Type: {0}/{1}\n", entity.ContentType.MediaType, entity.ContentType.MediaSubtype);

			if (entity is Multipart) {
				var multipart = (Multipart) entity;
				foreach (var part in multipart)
					DumpMimeTree (builder, part, depth + 1);
			} else if (entity is MessagePart) {
				DumpMimeTree (builder, ((MessagePart) entity).Message.Body, depth + 1);
			}
		}

		[Test]
		public void TestJwzMbox ()
		{
			var summary = File.ReadAllText ("../../TestData/mbox/jwz-summary.txt");
			var builder = new StringBuilder ();

			using (var stream = File.OpenRead ("../../TestData/mbox/jwz.mbox.txt")) {
				var parser = new MimeParser (stream, MimeFormat.Mbox);

				while (!parser.IsEndOfStream) {
					var message = parser.ParseMessage ();

					builder.AppendFormat ("{0}\n", parser.MboxMarker);
					if (message.From.Count > 0)
						builder.AppendFormat ("From: {0}\n", message.From);
					if (message.To.Count > 0)
						builder.AppendFormat ("To: {0}\n", message.To);
					builder.AppendFormat ("Subject: {0}\n", message.Subject);
					builder.AppendFormat ("Date: {0}\n", DateUtils.ToString (message.Date));
					DumpMimeTree (builder, message.Body, 0);
					builder.Append ("\n");
				}
			}

			string actual = builder.ToString ();

			// WORKAROUND: Mono's iso-2022-jp decoder seems broken?
			string iso2022jp = Encoding.GetEncoding ("iso-2022-jp").GetString (Convert.FromBase64String ("GyRAOjRGI0stGyhK"));
			if (iso2022jp != "佐藤豊")
				actual = actual.Replace (iso2022jp, "佐藤豊");

			Assert.AreEqual (summary, actual, "Summaries do not match for jwz.mbox");
		}
	}
}
