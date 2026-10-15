//
// DkimRelaxedBodyFilterTests.cs
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

using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class DkimRelaxedBodyFilterTests
	{
		[Test]
		public void TestWhiteSpaceBeforeNewLine ()
		{
			const string text = "text\t \r\n\t \r\ntext\t \r\n";
			const string expected = "text\r\n\r\ntext\r\n";
			var input = Encoding.ASCII.GetBytes (text);
			var filter = new DkimRelaxedBodyFilter ();
			int outputIndex, outputLength;
			byte[] output;
			string actual;

			output = filter.Flush (input, 0, input.Length, out outputIndex, out outputLength);
			actual = Encoding.ASCII.GetString (output, outputIndex, outputLength);

			Assert.That (actual, Is.EqualTo (expected));

			filter.Reset ();
		}

		[Test]
		public void TestTrimmingEmptyLines ()
		{
			const string text = "Hello!\r\n  \r\n\r\n";
			const string expected = "Hello!\r\n";
			var input = Encoding.ASCII.GetBytes (text);
			var filter = new DkimRelaxedBodyFilter ();
			int outputIndex, outputLength;
			byte[] output;
			string actual;

			output = filter.Flush (input, 0, input.Length, out outputIndex, out outputLength);
			actual = Encoding.ASCII.GetString (output, outputIndex, outputLength);

			Assert.That (actual, Is.EqualTo (expected));

			filter.Reset ();
		}

		[Test]
		public void TestMultipleWhiteSpacesPerLine ()
		{
			const string text = "This is a test of the relaxed body filter with  \t multiple \t  spaces\n";
			const string expected = "This is a test of the relaxed body filter with multiple spaces\n";
			var input = Encoding.ASCII.GetBytes (text);
			var filter = new DkimRelaxedBodyFilter ();
			int outputIndex, outputLength;
			byte[] output;
			string actual;

			output = filter.Flush (input, 0, input.Length, out outputIndex, out outputLength);
			actual = Encoding.ASCII.GetString (output, outputIndex, outputLength);

			Assert.That (actual, Is.EqualTo (expected));

			filter.Reset ();
		}

		[Test]
		public void TestCarriageReturnNotFollowedByLineFeed ()
		{
			const string text = "Hello\rWorld\n";
			const string expected = "Hello\rWorld\n";
			var input = Encoding.ASCII.GetBytes (text);
			var filter = new DkimRelaxedBodyFilter ();
			int outputIndex, outputLength;

			var output = filter.Flush (input, 0, input.Length, out outputIndex, out outputLength);
			var actual = Encoding.ASCII.GetString (output, outputIndex, outputLength);

			Assert.That (actual, Is.EqualTo (expected));
		}

		static string Filter (string text, out bool lastWasNewLine)
		{
			var input = Encoding.ASCII.GetBytes (text);
			var filter = new DkimRelaxedBodyFilter ();

			var output = filter.Flush (input, 0, input.Length, out int outputIndex, out int outputLength);
			lastWasNewLine = filter.LastWasNewLine;

			return Encoding.ASCII.GetString (output, outputIndex, outputLength);
		}

		[TestCase ("\r\r", "\r\r", false)]
		[TestCase ("\r", "\r", false)]
		[TestCase ("abc\r", "abc\r", false)]
		[TestCase ("\n\rX", "\r\n\rX", false)]
		[TestCase ("a\n\n\r\n\rb", "a\n\r\n\r\n\rb", false)]
		[TestCase ("a \rb", "a \rb", false)]
		[TestCase ("a\t\r \r\n", "a \r\r\n", true)]
		[TestCase ("a \r\n", "a\r\n", true)]
		[TestCase ("\r\r\n", "\r\r\n", true)]
		public void TestBareCarriageReturnIsContent (string text, string expected, bool expectedLastWasNewLine)
		{
			var actual = Filter (text, out bool lastWasNewLine);

			Assert.That (actual, Is.EqualTo (expected));
			Assert.That (lastWasNewLine, Is.EqualTo (expectedLastWasNewLine), "LastWasNewLine");
		}

		[Test]
		public void TestBareCarriageReturnSplitAcrossBuffers ()
		{
			var filter = new DkimRelaxedBodyFilter ();
			var first = Encoding.ASCII.GetBytes ("a\n\n \r");
			var second = Encoding.ASCII.GetBytes ("\r");
			var builder = new StringBuilder ();

			var output = filter.Filter (first, 0, first.Length, out int outputIndex, out int outputLength);
			builder.Append (Encoding.ASCII.GetString (output, outputIndex, outputLength));
			Assert.That (builder.ToString (), Is.EqualTo ("a\n"));

			output = filter.Flush (second, 0, second.Length, out outputIndex, out outputLength);
			builder.Append (Encoding.ASCII.GetString (output, outputIndex, outputLength));

			Assert.That (builder.ToString (), Is.EqualTo ("a\n\r\n \r\r"));
			Assert.That (filter.LastWasNewLine, Is.False, "LastWasNewLine");
		}

		[Test]
		public void TestNonEmptyBodyEndingWithMultipleNewLines ()
		{
			const string text = "This is a test of the relaxed body filter with a non-empty body ending with multiple new-lines\n\n\n";
			const string expected = "This is a test of the relaxed body filter with a non-empty body ending with multiple new-lines\n";
			var input = Encoding.ASCII.GetBytes (text);
			var filter = new DkimRelaxedBodyFilter ();
			int outputIndex, outputLength;
			byte[] output;
			string actual;

			output = filter.Flush (input, 0, input.Length, out outputIndex, out outputLength);
			actual = Encoding.ASCII.GetString (output, outputIndex, outputLength);

			Assert.That (actual, Is.EqualTo (expected));

			filter.Reset ();
		}

		[Test]
		public void TestManyBareLineFeedEmptyLines ()
		{
			// Each bare '\n' empty line is output as "\r\n", so the output is nearly twice the size of the input.
			var text = "a\n" + new string ('\n', 8192) + "b";
			var expected = "a\n" + new StringBuilder ().Insert (0, "\r\n", 8192) + "b";
			var input = Encoding.ASCII.GetBytes (text);
			var filter = new DkimRelaxedBodyFilter ();

			var output = filter.Flush (input, 0, input.Length, out int outputIndex, out int outputLength);
			var actual = Encoding.ASCII.GetString (output, outputIndex, outputLength);

			Assert.That (actual, Is.EqualTo (expected));
		}
	}
}
