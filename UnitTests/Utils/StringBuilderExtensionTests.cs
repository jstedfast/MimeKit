//
// StringBuilderExtensionTests.cs
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
using MimeKit.Utils;

namespace UnitTests.Utils {
	[TestFixture]
	public class StringBuilderExtensionTests
	{
		[Test]
		public void TestLineWrap ()
		{
			var builder = new StringBuilder ("This is supposed to be a really long string of text that is about to be folded ");
			const string expected1 = "This is supposed to be a really long string of text that is about to be folded\n ";
			const string expected2 = "This is supposed to be a really long string of text that is about to be folded\n\t";
			var format = FormatOptions.Default.Clone ();

			format.NewLineFormat = NewLineFormat.Unix;

			builder.LineWrap (format);

			Assert.That (builder.ToString (), Is.EqualTo (expected1), "#1");

			builder.Length -= 2;

			builder.LineWrap (format);

			Assert.That (builder.ToString (), Is.EqualTo (expected2), "#2");
		}

		[Test]
		public void TestAppendTokens ()
		{
			var builder = new StringBuilder ("Authentication-Results:");
			var format = FormatOptions.Default.Clone ();
			var tokens = new List<string> ();
			int lineLength = builder.Length;

			format.NewLineFormat = NewLineFormat.Unix;

			tokens.Add (" ");
			tokens.Add ("this-is-a-really-long-parameter-name");
			tokens.Add ("=");
			tokens.Add ("this-is-a-really-long-parameter-value");

			builder.AppendTokens (format, ref lineLength, tokens);

			Assert.That (builder.ToString (), Is.EqualTo ("Authentication-Results: this-is-a-really-long-parameter-name=\n\tthis-is-a-really-long-parameter-value"));
		}

		[Test]
		public void TestAppendFoldedWithQuotedString ()
		{
			const string expected = "This is about to get a quoted string appended to it:\n \"and this is a \\\"quoted string\\\" that must not get broken up!\" Got it? Good.\n There should be another wrap in here...";
			var builder = new StringBuilder ("This is about to get a quoted string appended ");
			var format = FormatOptions.Default.Clone ();
			int lineLength = builder.Length;

			format.NewLineFormat = NewLineFormat.Unix;

			builder.AppendFolded (format, false, "to it: \"and this is a \\\"quoted string\\\" that must not get broken up!\" Got it? Good. There should be another wrap in here...", ref lineLength);

			Assert.That (builder.ToString (), Is.EqualTo (expected));
			Assert.That (lineLength, Is.EqualTo (40));
		}

#if DEBUG
		[Test]
		public void TestAppendCString ()
		{
			const string expected = "\\0\\x01\\x02\\x03\\x04\\x05\\x06\\a\\b\\t\\n\\v\\x0c\\r\\x0e\\x0f\\x10\\x11\\x12\\x13\\x14\\x15\\x16\\x17\\x18\\x19\\x1a\\x1b\\x1c\\x1d\\x1e\\x1f ";
			var builder = new StringBuilder ();
			var cstr = new byte[0x21];

			for (byte i = 0; i < 0x21; i++)
				cstr[i] = i;

			builder.AppendCString (cstr, 0, cstr.Length);

			Assert.That (builder.ToString (), Is.EqualTo (expected));
		}
#endif
	}
}
