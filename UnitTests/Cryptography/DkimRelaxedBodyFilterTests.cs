//
// DkimRelaxedBodyFilterTests.cs
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
	}
}
