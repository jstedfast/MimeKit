//
// Rfc2047Tests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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
using System.Text;

using NUnit.Framework;

using MimeKit;
using MimeKit.Utils;

namespace UnitTests.Utils {
	[TestFixture]
	public class Rfc2047Tests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var text = Encoding.UTF8.GetBytes ("this is some text");

			// DecodePhrase
			Assert.Throws<ArgumentNullException> (() => Rfc2047.DecodePhrase (null, text, 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.DecodePhrase (ParserOptions.Default, null, 0, text.Length));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.DecodePhrase (ParserOptions.Default, text, -1, text.Length));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.DecodePhrase (ParserOptions.Default, text, 0, -1));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.DecodePhrase (null, 0, text.Length));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.DecodePhrase (text, -1, text.Length));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.DecodePhrase (text, 0, -1));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.DecodePhrase (null, text));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.DecodePhrase (ParserOptions.Default, null));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.DecodePhrase (null));

			// DecodeText
			Assert.Throws<ArgumentNullException> (() => Rfc2047.DecodeText (null, text, 0, text.Length));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.DecodeText (ParserOptions.Default, null, 0, text.Length));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.DecodeText (ParserOptions.Default, text, -1, text.Length));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.DecodeText (ParserOptions.Default, text, 0, -1));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.DecodeText (null, 0, text.Length));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.DecodeText (text, -1, text.Length));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.DecodeText (text, 0, -1));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.DecodeText (null, text));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.DecodeText (ParserOptions.Default, null));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.DecodeText (null));

			// EncodePhrase
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (null, Encoding.UTF8, "phrase"));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (FormatOptions.Default, null, "phrase"));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (FormatOptions.Default, Encoding.UTF8, null));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (null, "phrase"));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (Encoding.UTF8, null));

			// EncodeText
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (null, Encoding.UTF8, "text"));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (FormatOptions.Default, null, "text"));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (FormatOptions.Default, Encoding.UTF8, null));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (null, "text"));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (Encoding.UTF8, null));
		}

		[Test]
		public void TestEncodedWordEmptyCharset ()
		{
			const string text = "blurdy bloop =??q?no_charset?= beep boop";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.AreEqual (text, result);

			result = Rfc2047.DecodeText (buffer);
			Assert.AreEqual (text, result);
		}

		[Test]
		public void TestEncodedWordEmptyCharsetWithLang ()
		{
			const string text = "blurdy bloop =?*en?q?no_charset?= beep boop";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.AreEqual (text, result);

			result = Rfc2047.DecodeText (buffer);
			Assert.AreEqual (text, result);
		}

		[Test]
		public void TestEncodedWordWithLang ()
		{
			const string text = "blurdy bloop =?iso-8859-1*en?q?this_is_english?= beep boop";
			const string expected = "blurdy bloop this is english beep boop";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.AreEqual (expected, result);

			result = Rfc2047.DecodeText (buffer);
			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestEncodedWordInvalidEncoding ()
		{
			const string text = "blurdy bloop =?iso-8859-1?x?invalid_encoding?= beep boop";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.AreEqual (text, result);

			result = Rfc2047.DecodeText (buffer);
			Assert.AreEqual (text, result);
		}

		[Test]
		public void TestEncodedWordIncompletePayload ()
		{
			const string text = "blurdy bloop =?iso-8859-1?x?invalid_encoding";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.AreEqual (text, result);

			result = Rfc2047.DecodeText (buffer);
			Assert.AreEqual (text, result);
		}

		[Test]
		public void TestEncodedWordIncompleteCharset ()
		{
			const string text = "blurdy bloop =?iso-8859-1";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.AreEqual (text, result);

			result = Rfc2047.DecodeText (buffer);
			Assert.AreEqual (text, result);
		}
	}
}
