//
// MimeUtilsTests.cs
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

using MimeKit.Utils;

namespace UnitTests.Utils {
	[TestFixture]
	public class MimeUtilsTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var buffer = new byte[1024];

			Assert.Throws<ArgumentNullException> (() => MimeUtils.GenerateMessageId (null), "MimeUtils.GenerateMessageId (null)");
			Assert.Throws<ArgumentException> (() => MimeUtils.GenerateMessageId (string.Empty), "MimeUtils.GenerateMessageId (string.Empty)");

			Assert.Throws<ArgumentNullException> (() => MimeUtils.EnumerateReferences (null), "MimeUtils.EnumerateReferences (null)");
			Assert.Throws<ArgumentNullException> (() => MimeUtils.EnumerateReferences (null, 0, 0).FirstOrDefault (), "MimeUtils.EnumerateReferences (null, 0, 0)");
			Assert.Throws<ArgumentOutOfRangeException> (() => MimeUtils.EnumerateReferences (buffer, -1, 0).FirstOrDefault (), "MimeUtils.EnumerateReferences (buffer, -1, 0)");
			Assert.Throws<ArgumentOutOfRangeException> (() => MimeUtils.EnumerateReferences (buffer, buffer.Length + 1, 0).FirstOrDefault (), "MimeUtils.EnumerateReferences (buffer, buffer.Length + 1, 0)");
			Assert.Throws<ArgumentOutOfRangeException> (() => MimeUtils.EnumerateReferences (buffer, 0, -1).FirstOrDefault (), "MimeUtils.EnumerateReferences (buffer, 0, -1)");
			Assert.Throws<ArgumentOutOfRangeException> (() => MimeUtils.EnumerateReferences (buffer, 0, buffer.Length + 1).FirstOrDefault (), "MimeUtils.EnumerateReferences (buffer, 0, buffer.Length + 1)");

			Assert.Throws<ArgumentNullException> (() => MimeUtils.ParseMessageId (null), "MimeUtils.ParseMessageId (null)");
			Assert.Throws<ArgumentNullException> (() => MimeUtils.ParseMessageId (null, 0, 0), "MimeUtils.ParseMessageId (null, 0, 0)");
			Assert.Throws<ArgumentOutOfRangeException> (() => MimeUtils.ParseMessageId (buffer, -1, 0), "MimeUtils.ParseMessageId (buffer, -1, 0)");
			Assert.Throws<ArgumentOutOfRangeException> (() => MimeUtils.ParseMessageId (buffer, buffer.Length + 1, 0), "MimeUtils.ParseMessageId (buffer, buffer.Length + 1, 0)");
			Assert.Throws<ArgumentOutOfRangeException> (() => MimeUtils.ParseMessageId (buffer, 0, -1), "MimeUtils.ParseMessageId (buffer, 0, -1)");
			Assert.Throws<ArgumentOutOfRangeException> (() => MimeUtils.ParseMessageId (buffer, 0, buffer.Length + 1), "MimeUtils.ParseMessageId (buffer, 0, buffer.Length + 1)");

			Assert.Throws<ArgumentNullException> (() => MimeUtils.AppendQuoted (null, "text"), "MimeUtils.AppendQuoted (null, value)");
			Assert.Throws<ArgumentNullException> (() => MimeUtils.AppendQuoted (new StringBuilder (), null), "MimeUtils.AppendQuoted (builder, null)");
			//Assert.Throws<ArgumentNullException> (() => MimeUtils.Quote ((ReadOnlySpan<char>) null), "MimeUtils.Quote (null)");
			Assert.Throws<ArgumentNullException> (() => MimeUtils.Quote ((string) null), "MimeUtils.Quote (null)");
			Assert.Throws<ArgumentNullException> (() => MimeUtils.Unquote (null), "MimeUtils.Unquote (null)");
		}

		static readonly string[] GoodReferences = {
			//" <local (comment) . (comment) part (comment) @ (comment) localhost (comment) . (comment) localdomain (comment) >", "local.part@localhost.localdomain",
			"<local-part@domain1@domain2>",                                                                                     "local-part@domain1@domain2",
			"<local-part@>",                                                                                                    "local-part@",
			"<local-part>",                                                                                                     "local-part",
			"<:invalid-local-part;@domain.com>",                                                                                ":invalid-local-part;@domain.com",
		};

		[Test]
		public void TestParseGoodReferences ()
		{
			for (int i = 0; i < GoodReferences.Length; i += 2) {
				var reference = MimeUtils.EnumerateReferences (GoodReferences[i]).FirstOrDefault ();

				Assert.That (reference, Is.EqualTo (GoodReferences[i + 1]), $"Incorrectly parsed reference '{GoodReferences[i]}'.");

				reference = MimeUtils.ParseMessageId (GoodReferences[i]);

				Assert.That (reference, Is.EqualTo (GoodReferences[i + 1]), $"Incorrectly parsed message-id '{GoodReferences[i]}'.");
			}
		}

		static readonly string[] BrokenReferences = {
			" (this is an unterminated comment...",
			"(this is just a comment)",
			"<",
			"<local-part",
			"<local-part;",
			"<local-part@ (unterminated comment...",
			"<local-part@",
			"<local-part @ bad-domain (comment) . (comment com",
			"<local-part@[127.0"
		};

		[Test]
		public void TestParseBrokenReferences ()
		{
			for (int i = 0; i < BrokenReferences.Length; i++) {
				var reference = MimeUtils.EnumerateReferences (BrokenReferences[i]).FirstOrDefault ();

				Assert.That (reference, Is.Null, $"MimeUtils.EnumerateReferences(\"{BrokenReferences[i]}\")");

				reference = MimeUtils.ParseMessageId (BrokenReferences[i]);

				Assert.That (reference, Is.Null, $"MimeUtils.ParseMessageId (\"{BrokenReferences[i]}\")");
			}
		}

		[Test]
		public void TestTryParseVersion ()
		{
			Version version;

			Assert.That (MimeUtils.TryParse (" 1 (comment) .\t0\r\n", out version), Is.True, "1.0");
			Assert.That (version.ToString (), Is.EqualTo ("1.0"));

			Assert.That (MimeUtils.TryParse (" 1 (comment) .\t0\r\n .0\r\n", out version), Is.True, "1.0.0");
			Assert.That (version.ToString (), Is.EqualTo ("1.0.0"));

			Assert.That (MimeUtils.TryParse (" 1 (comment) .\t0\r\n .0.0\r\n", out version), Is.True, "1.0.0.0");
			Assert.That (version.ToString (), Is.EqualTo ("1.0.0.0"));

			Assert.That (MimeUtils.TryParse ("1", out Version _), Is.False, "1");
			Assert.That (MimeUtils.TryParse ("1.2.3.4.5", out Version _), Is.False, "1.2.3.4.5");
			Assert.That (MimeUtils.TryParse ("1x2.3", out Version _), Is.False, "1x2.3");
			Assert.That (MimeUtils.TryParse ("(unterminated comment", out Version _), Is.False, "unterminated comment");
			Assert.That (MimeUtils.TryParse ("1 (unterminated comment", out Version _), Is.False, "1 + unterminated comment");
		}

		[Test]
		public void TestGenerateMessageIdWithInternationalDomain ()
		{
			const string domain = "Mjölnir";

			var msgid = MimeUtils.GenerateMessageId (domain);
			int at = msgid.IndexOf ('@');
			var idn = msgid.Substring (at + 1);

			Assert.That (idn, Is.EqualTo ("xn--mjlnir-xxa"));
		}

		[Test]
		public void TestAppendQuoted ()
		{
			const string expected = "\"This is a multi-line quoted string with \\\\backslashes\\\\ and an escaped dquote (\\\") inside of it.\"";
			const string text = "This is a multi-line quoted string with \\backslashes\\ and an escaped dquote (\") inside of it.";
			var builder = new StringBuilder ();

			MimeUtils.AppendQuoted (builder, text);

			Assert.That (builder.ToString (), Is.EqualTo (expected));
		}

		[Test]
		public void TestUnquote ()
		{
			const string quoted = "\"This is a multi-line quoted string with\r\n\t\\\\backslashes\\\\ and an escaped dquote (\\\") inside of it.\"";
			const string expected = "This is a multi-line quoted string with \\backslashes\\ and an escaped dquote (\") inside of it.";
			var unquoted = MimeUtils.Unquote (quoted, true);

			Assert.That (unquoted, Is.EqualTo (expected), "Unquote(string)");

			var bytes = Encoding.ASCII.GetBytes (quoted);
			var result = MimeUtils.Unquote (bytes, 0, bytes.Length, true);
			unquoted = Encoding.ASCII.GetString (result);

			Assert.That (unquoted, Is.EqualTo (expected), "Unquote(byte[])");
		}
	}
}
