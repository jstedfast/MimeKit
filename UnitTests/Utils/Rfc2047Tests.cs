//
// Rfc2047Tests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2022 .NET Foundation and Contributors
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
		public void TestDecodeEmptyString ()
		{
			var empty = new byte[0];

			Assert.AreEqual (string.Empty, Rfc2047.DecodePhrase (empty));
			Assert.AreEqual (string.Empty, Rfc2047.DecodePhrase (empty, 0, 0));
			Assert.AreEqual (string.Empty, Rfc2047.DecodePhrase (ParserOptions.Default, empty));
			Assert.AreEqual (string.Empty, Rfc2047.DecodePhrase (ParserOptions.Default, empty, 0, 0));
			Assert.AreEqual (string.Empty, Rfc2047.DecodePhrase (ParserOptions.Default, empty, 0, 0, out _));

			Assert.AreEqual (string.Empty, Rfc2047.DecodeText (empty));
			Assert.AreEqual (string.Empty, Rfc2047.DecodeText (empty, 0, 0));
			Assert.AreEqual (string.Empty, Rfc2047.DecodeText (ParserOptions.Default, empty));
			Assert.AreEqual (string.Empty, Rfc2047.DecodeText (ParserOptions.Default, empty, 0, 0));
			Assert.AreEqual (string.Empty, Rfc2047.DecodeText (ParserOptions.Default, empty, 0, 0, out _));
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
			const string text = "blurdy bloop =?iso-8859-1?q?invalid_encoding";
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

		[Test]
		public void TestEncodeControls ()
		{
			const string expected = "I'm so happy! =?utf-8?q?=07?= I love MIME so much =?utf-8?q?=07=07!?= Isn't it great?";
			const string text = "I'm so happy! \a I love MIME so much \a\a! Isn't it great?";
			string result;

			result = Encoding.ASCII.GetString (Rfc2047.EncodePhrase (Encoding.UTF8, text));
			Assert.AreEqual (expected, result, "EncodePhrase");

			result = Encoding.ASCII.GetString (Rfc2047.EncodeText (Encoding.UTF8, text));
			Assert.AreEqual (expected, result, "EncodeText");
		}

		[Test]
		public void TestEncodeSurrogatePair ()
		{
			const string expected = "I'm so happy! =?utf-8?b?8J+YgA==?= I love MIME so much =?utf-8?b?4p2k77iP4oCN8J+UpSE=?= Isn't it great?";
			const string text = "I'm so happy! 😀 I love MIME so much ❤️‍🔥! Isn't it great?";
			string result;

			result = Encoding.ASCII.GetString (Rfc2047.EncodePhrase (Encoding.UTF8, text));
			Assert.AreEqual (expected, result, "EncodePhrase");

			result = Encoding.ASCII.GetString (Rfc2047.EncodeText (Encoding.UTF8, text));
			Assert.AreEqual (expected, result, "EncodeText");
		}

		[Test]
		public void TestEncodeWrongCharset ()
		{
			const string expected = "I'm so happy! =?utf-8?b?5ZCN44GM44OJ44Oh44Kk44Oz?= I love MIME so much =?utf-8?b?4p2k77iP4oCN8J+UpSE=?= Isn't it great?";
			const string text = "I'm so happy! 名がドメイン I love MIME so much ❤️‍🔥! Isn't it great?";
			var latin1 = Encoding.GetEncoding ("iso-8859-1", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
			string result;

			result = Encoding.ASCII.GetString (Rfc2047.EncodePhrase (latin1, text));
			Assert.AreEqual (expected, result, "EncodePhrase");

			result = Encoding.ASCII.GetString (Rfc2047.EncodeText (latin1, text));
			Assert.AreEqual (expected, result, "EncodeText");
		}

		[Test]
		public void TestFoldMultiLineHeaderValue ()
		{
			const string expected = " This is a multi-line\r\n header value.\r\n";
			const string text = "This is a multi-line\r\nheader value.";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Dos;

			var result = Encoding.ASCII.GetString (Rfc2047.FoldUnstructuredHeader (options, "Subject", Encoding.ASCII.GetBytes (text)));
			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestFoldPreFoldedHeaderValue ()
		{
			const string expected = " This is a pre\r\n folded header value.\r\n";
			const string text = "This is a pre\r\n folded header value.";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Dos;

			var result = Encoding.ASCII.GetString (Rfc2047.FoldUnstructuredHeader (options, "Subject", Encoding.ASCII.GetBytes (text)));
			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestFoldReallyLongWordToken ()
		{
			const string expected = " This header value has a\r\n really-really-really-really-long-rfc0822-word-token-that-exceeds-the-max-allo\r\n wable-line-length-and-must-be-folded lets see what MimeKit does...\r\n";
			const string text = "This header value has a really-really-really-really-long-rfc0822-word-token-that-exceeds-the-max-allowable-line-length-and-must-be-folded lets see what MimeKit does...";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Dos;

			var result = Encoding.ASCII.GetString (Rfc2047.FoldUnstructuredHeader (options, "Subject", Encoding.ASCII.GetBytes (text)));
			Assert.AreEqual (expected, result);
		}
	}
}
