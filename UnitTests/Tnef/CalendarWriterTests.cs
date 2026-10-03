//
// CalendarWriterTests.cs
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

using System.Text;

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class CalendarWriterTests
	{
		static string Write (Action<CalendarWriter> write)
		{
			using (var stream = new MemoryStream ()) {
				write (new CalendarWriter (stream));

				return Encoding.UTF8.GetString (stream.ToArray ());
			}
		}

		[Test]
		public void TestWriteProperty ()
		{
			var text = Write (writer => {
				writer.BeginComponent ("VEVENT");
				writer.WriteProperty ("DTSTART", "20240101T000000Z");
				writer.EndComponent ("VEVENT");
			});

			Assert.That (text, Is.EqualTo ("BEGIN:VEVENT\r\nDTSTART:20240101T000000Z\r\nEND:VEVENT\r\n"));
		}

		[Test]
		public void TestTextEscaping ()
		{
			var text = Write (writer => writer.WriteTextProperty ("SUMMARY", "a\\b;c,d\r\ne\nf\rg\u0001h"));

			Assert.That (text, Is.EqualTo ("SUMMARY:a\\\\b\\;c\\,d\\ne\\nf\\ng" + "h\r\n"));
		}

		[Test]
		public void TestParameterQuoting ()
		{
			var text = Write (writer => {
				writer.BeginProperty ("ATTENDEE");
				writer.WriteParameter ("CN", "Doe, John");
				writer.WriteParameter ("ROLE", "REQ-PARTICIPANT");
				writer.WriteParameter ("X-NAME", "say \"hi\"");
				writer.WriteValue ("mailto:j@x.org");
			});

			Assert.That (text, Is.EqualTo ("ATTENDEE;CN=\"Doe, John\";ROLE=REQ-PARTICIPANT;X-NAME=say 'hi':mailto:j@x.org\r\n"));
		}

		[Test]
		public void TestFolding ()
		{
			var value = new string ('x', 200);
			var text = Write (writer => writer.WriteTextProperty ("SUMMARY", value));
			var lines = text.Split (new[] { "\r\n" }, StringSplitOptions.None);

			Assert.That (lines[lines.Length - 1], Is.Empty);

			for (int i = 0; i < lines.Length - 1; i++) {
				Assert.That (Encoding.UTF8.GetByteCount (lines[i]), Is.LessThanOrEqualTo (75), $"line {i}");

				if (i > 0)
					Assert.That (lines[i][0], Is.EqualTo (' '));
			}

			Assert.That (text.Replace ("\r\n ", string.Empty), Is.EqualTo ("SUMMARY:" + value + "\r\n"));
		}

		[Test]
		public void TestFoldingDoesNotSplitMultibyteSequences ()
		{
			// Each character is 3 octets in UTF-8, so the fold points never line up with a character boundary by chance.
			var value = new string ('\u20AC', 100);
			var text = Write (writer => writer.WriteTextProperty ("SUMMARY", "x" + value));
			var bytes = Encoding.UTF8.GetBytes (text);
			int lineLength = 0;

			for (int i = 0; i < bytes.Length; i++) {
				if (bytes[i] == (byte) '\r') {
					Assert.That (bytes[i + 1], Is.EqualTo ((byte) '\n'));
					Assert.That (lineLength, Is.LessThanOrEqualTo (75));

					// The first octet after the fold must not be a UTF-8 continuation octet.
					if (i + 3 < bytes.Length)
						Assert.That (bytes[i + 3] & 0xC0, Is.Not.EqualTo (0x80));

					lineLength = 0;
					i++;
					continue;
				}

				lineLength++;
			}

			Assert.That (text.Replace ("\r\n ", string.Empty), Is.EqualTo ("SUMMARY:x" + value + "\r\n"));
		}

		[Test]
		public void TestFormatting ()
		{
			var date = new DateTime (2024, 7, 4, 13, 5, 9);

			Assert.That (CalendarWriter.FormatDateTime (date, true), Is.EqualTo ("20240704T130509Z"));
			Assert.That (CalendarWriter.FormatDateTime (date, false), Is.EqualTo ("20240704T130509"));
			Assert.That (CalendarWriter.FormatDate (date), Is.EqualTo ("20240704"));
			Assert.That (CalendarWriter.FormatUtcOffset (-300), Is.EqualTo ("-0500"));
			Assert.That (CalendarWriter.FormatUtcOffset (330), Is.EqualTo ("+0530"));
			Assert.That (CalendarWriter.FormatUtcOffset (0), Is.EqualTo ("+0000"));
		}

		[Test]
		public void TestPropertyStateErrors ()
		{
			using (var stream = new MemoryStream ()) {
				var writer = new CalendarWriter (stream);

				Assert.Throws<InvalidOperationException> (() => writer.WriteValue ("x"));
				Assert.Throws<InvalidOperationException> (() => writer.WriteTextValue ("x"));
				Assert.Throws<InvalidOperationException> (() => writer.WriteParameter ("A", "B"));

				writer.BeginProperty ("X");

				Assert.Throws<InvalidOperationException> (() => writer.BeginProperty ("Y"));
			}
		}
	}
}
