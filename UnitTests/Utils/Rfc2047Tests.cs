//
// Rfc2047Tests.cs
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
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (null, Encoding.UTF8, "phrase", 0, 6));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (FormatOptions.Default, null, "phrase", 0, 6));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (FormatOptions.Default, Encoding.UTF8, null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.EncodePhrase (FormatOptions.Default, Encoding.UTF8, "phrase", -1, 6));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.EncodePhrase (FormatOptions.Default, Encoding.UTF8, "phrase", 0, 7));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (null, "phrase", 0, 6));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (Encoding.UTF8, null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.EncodePhrase (Encoding.UTF8, "phrase", -1, 6));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.EncodePhrase (Encoding.UTF8, "phrase", 0, 7));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (null, Encoding.UTF8, "phrase"));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (FormatOptions.Default, null, "phrase"));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (FormatOptions.Default, Encoding.UTF8, null));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (null, "phrase"));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodePhrase (Encoding.UTF8, null));

			// EncodeText
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (null, Encoding.UTF8, "text", 0, 4));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (FormatOptions.Default, null, "text", 0, 4));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (FormatOptions.Default, Encoding.UTF8, null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.EncodeText (FormatOptions.Default, Encoding.UTF8, "text", -1, 4));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.EncodeText (FormatOptions.Default, Encoding.UTF8, "text", 0, 5));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (null, "text", 0, 4));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (Encoding.UTF8, null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.EncodeText (Encoding.UTF8, "text", -1, 4));
			Assert.Throws<ArgumentOutOfRangeException> (() => Rfc2047.EncodeText (Encoding.UTF8, "text", 0, 5));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (null, Encoding.UTF8, "text"));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (FormatOptions.Default, null, "text"));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (FormatOptions.Default, Encoding.UTF8, null));

			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (null, "text"));
			Assert.Throws<ArgumentNullException> (() => Rfc2047.EncodeText (Encoding.UTF8, null));
		}

		[Test]
		public void TestDecodeEmptyString ()
		{
			var empty = Array.Empty<byte> ();

			Assert.That (Rfc2047.DecodePhrase (empty), Is.EqualTo (string.Empty));
			Assert.That (Rfc2047.DecodePhrase (empty, 0, 0), Is.EqualTo (string.Empty));
			Assert.That (Rfc2047.DecodePhrase (ParserOptions.Default, empty), Is.EqualTo (string.Empty));
			Assert.That (Rfc2047.DecodePhrase (ParserOptions.Default, empty, 0, 0), Is.EqualTo (string.Empty));
			Assert.That (Rfc2047.DecodePhrase (ParserOptions.Default, empty, 0, 0, out _), Is.EqualTo (string.Empty));

			Assert.That (Rfc2047.DecodeText (empty), Is.EqualTo (string.Empty));
			Assert.That (Rfc2047.DecodeText (empty, 0, 0), Is.EqualTo (string.Empty));
			Assert.That (Rfc2047.DecodeText (ParserOptions.Default, empty), Is.EqualTo (string.Empty));
			Assert.That (Rfc2047.DecodeText (ParserOptions.Default, empty, 0, 0), Is.EqualTo (string.Empty));
			Assert.That (Rfc2047.DecodeText (ParserOptions.Default, empty, 0, 0, out _), Is.EqualTo (string.Empty));
		}

		[Test]
		public void TestDecodeEncodedWordEmptyCharset ()
		{
			const string text = "blurdy bloop =??q?no_charset?= beep boop";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.That (result, Is.EqualTo (text));

			result = Rfc2047.DecodeText (buffer);
			Assert.That (result, Is.EqualTo (text));
		}

		[Test]
		public void TestDecodeEncodedWordEmptyCharsetWithLang ()
		{
			const string text = "blurdy bloop =?*en?q?no_charset?= beep boop";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.That (result, Is.EqualTo (text));

			result = Rfc2047.DecodeText (buffer);
			Assert.That (result, Is.EqualTo (text));
		}

		[Test]
		public void TestDecodeEncodedWordWithLang ()
		{
			const string text = "blurdy bloop =?iso-8859-1*en?q?this_is_english?= beep boop";
			const string expected = "blurdy bloop this is english beep boop";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.That (result, Is.EqualTo (expected));

			result = Rfc2047.DecodeText (buffer);
			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestDecodeEncodedWordInvalidEncoding ()
		{
			const string text = "blurdy bloop =?iso-8859-1?x?invalid_encoding?= beep boop";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.That (result, Is.EqualTo (text));

			result = Rfc2047.DecodeText (buffer);
			Assert.That (result, Is.EqualTo (text));
		}

		[Test]
		public void TestDecodeEncodedWordInvalidMultiCharacterEncoding ()
		{
			const string text = "blurdy bloop =?iso-8859-1?qb?invalid_encoded_word?= beep boop";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.That (result, Is.EqualTo (text));

			result = Rfc2047.DecodeText (buffer);
			Assert.That (result, Is.EqualTo (text));
		}

		[Test]
		public void TestDecodeEncodedWordIncompletePayload ()
		{
			const string text = "blurdy bloop =?iso-8859-1?q?invalid_encoding";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.That (result, Is.EqualTo (text));

			result = Rfc2047.DecodeText (buffer);
			Assert.That (result, Is.EqualTo (text));
		}

		[Test]
		public void TestDecodeEncodedWordIncompleteCharset ()
		{
			const string text = "blurdy bloop =?iso-8859-1";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.That (result, Is.EqualTo (text));

			result = Rfc2047.DecodeText (buffer);
			Assert.That (result, Is.EqualTo (text));
		}

		[Test]
		public void TestDecodeEncodedWordInvalidCharsetName ()
		{
			const string text = "blurdy bloop =?isö-8859-1?q?invalid_charset_name?= beep boop";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.That (result, Is.EqualTo (text));

			result = Rfc2047.DecodeText (buffer);
			Assert.That (result, Is.EqualTo (text));
		}

		[Test]
		public void TestDecodeEncodedWordInvalidLanguageCode ()
		{
			const string text = "blurdy bloop =?iso-8859-1*eñ-US?q?invalid_charset_name?= beep boop";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.That (result, Is.EqualTo (text));

			result = Rfc2047.DecodeText (buffer);
			Assert.That (result, Is.EqualTo (text));
		}

		[Test]
		public void TestDecodeEncodedWordEmbeddedInAnotherWord ()
		{
			const string text = "blurdy bloop=?iso-8859-1?q?_encoded_word_?=beep boop";
			const string expected = "blurdy bloop encoded word beep boop";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;

			result = Rfc2047.DecodePhrase (buffer);
			Assert.That (result, Is.EqualTo (expected));

			result = Rfc2047.DecodeText (buffer);
			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestDecodeMultipleEncodedWordsWithCommonCodePage ()
		{
			const string text = "=?iso-8859-1?q?latin1_?= =?utf-8?q?unicode_?= =?iso-8859-1?q?and_latin1_again?=";
			const string expected = "latin1 unicode and latin1 again";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;
			int codepage;

			result = Rfc2047.DecodePhrase (ParserOptions.Default, buffer, 0, buffer.Length, out codepage);
			Assert.That (result, Is.EqualTo (expected), "DecodePhrase");
			Assert.That (codepage, Is.EqualTo (28591), "DecodePhrase");

			result = Rfc2047.DecodeText (ParserOptions.Default, buffer, 0, buffer.Length, out codepage);
			Assert.That (result, Is.EqualTo (expected), "DecodeText");
			Assert.That (codepage, Is.EqualTo (28591), "DecodeText");
		}

		// TODO: When no common codepage can be found, this logic should be smarter?
		[Test]
		public void TestDecodeMultipleEncodedWordsWithoutCommonCodePage ()
		{
			const string text = "=?iso-8859-1?q?latin1_?= =?iso-8859-2?q?latin2_?= =?iso-8859-3?q?latin3?=";
			const string expected = "latin1 latin2 latin3";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;
			int codepage;

			result = Rfc2047.DecodePhrase (ParserOptions.Default, buffer, 0, buffer.Length, out codepage);
			Assert.That (result, Is.EqualTo (expected), "DecodePhrase");
			Assert.That (codepage, Is.EqualTo (28591), "DecodePhrase");

			result = Rfc2047.DecodeText (ParserOptions.Default, buffer, 0, buffer.Length, out codepage);
			Assert.That (result, Is.EqualTo (expected), "DecodeText");
			Assert.That (codepage, Is.EqualTo (28591), "DecodeText");
		}

		[Test]
		public void TestDecodeEnsuresCodePageCapacity ()
		{
			const string text = "=?us-ascii?q?0?= =?iso-8859-1?q?1?= =?iso-8859-2?q?2?= =?iso-8859-3?q?3?= =?iso-8859-4?q?4?= =?iso-8859-5?q?5?= =?iso-8859-6?q?6?= =?iso-8859-7?q?7?= =?iso-8859-8?q?8?= =?iso-8859-9?q?9?= =?koi8-r?q?a?= =?koi8-u?q?b?= =?big5?q?c?= =?euc-cn?q?d?= =?euc-kr?q?e?= =?utf-8?q?f?= =?gb2312?q?g?=";
			const string expected = "0123456789abcdefg";
			var buffer = Encoding.UTF8.GetBytes (text);
			string result;
			int codepage;

			result = Rfc2047.DecodePhrase (ParserOptions.Default, buffer, 0, buffer.Length, out codepage);
			Assert.That (result, Is.EqualTo (expected), "DecodePhrase");
			Assert.That (codepage, Is.EqualTo (20127), "DecodePhrase"); // FIXME: it should not select us-ascii

			result = Rfc2047.DecodeText (ParserOptions.Default, buffer, 0, buffer.Length, out codepage);
			Assert.That (result, Is.EqualTo (expected), "DecodeText");
			Assert.That (codepage, Is.EqualTo (20127), "DecodeText"); // FIXME: it should not select us-ascii
		}

		[Test]
		public void TestEncodeControls ()
		{
			const string expected = "I'm so happy! =?utf-8?q?=07?= I love MIME so much =?utf-8?q?=07=07!?= Isn't it great?";
			const string text = "I'm so happy! \a I love MIME so much \a\a! Isn't it great?";
			string result;

			result = Encoding.ASCII.GetString (Rfc2047.EncodePhrase (Encoding.UTF8, text));
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase(Encoding, string)");

			result = Encoding.ASCII.GetString (Rfc2047.EncodePhrase (Encoding.UTF8, text, 0, text.Length));
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase(Encoding, string, int, int)");

			result = Encoding.ASCII.GetString (Rfc2047.EncodeText (Encoding.UTF8, text));
			Assert.That (result, Is.EqualTo (expected), "EncodeText(Encoding, string)");

			result = Encoding.ASCII.GetString (Rfc2047.EncodeText (Encoding.UTF8, text, 0, text.Length));
			Assert.That (result, Is.EqualTo (expected), "EncodeText(Encoding, string, int, int)");
		}

		[Test]
		public void TestEncodeSurrogatePair ()
		{
			const string expected = "I'm so happy! =?utf-8?b?8J+YgA==?= I love MIME so much =?utf-8?b?4p2k77iP4oCN8J+UpSE=?= Isn't it great?";
			const string text = "I'm so happy! 😀 I love MIME so much ❤️‍🔥! Isn't it great?";
			string result;

			result = Encoding.ASCII.GetString (Rfc2047.EncodePhrase (Encoding.UTF8, text));
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase(Encoding, string)");

			result = Encoding.ASCII.GetString (Rfc2047.EncodePhrase (Encoding.UTF8, text, 0, text.Length));
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase(Encoding, string, int, int)");

			result = Encoding.ASCII.GetString (Rfc2047.EncodeText (Encoding.UTF8, text));
			Assert.That (result, Is.EqualTo (expected), "EncodeText(Encoding, string)");

			result = Encoding.ASCII.GetString (Rfc2047.EncodeText (Encoding.UTF8, text, 0, text.Length));
			Assert.That (result, Is.EqualTo (expected), "EncodeText(Encoding, string, int, int)");
		}

		[Test]
		public void TestEncodeWrongCharset ()
		{
			const string expected = "I'm so happy! =?utf-8?b?5ZCN44GM44OJ44Oh44Kk44Oz?= I love MIME so much =?utf-8?b?4p2k77iP4oCN8J+UpSE=?= Isn't it great?";
			const string text = "I'm so happy! 名がドメイン I love MIME so much ❤️‍🔥! Isn't it great?";
			var latin1 = Encoding.GetEncoding ("iso-8859-1", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
			string result;

			result = Encoding.ASCII.GetString (Rfc2047.EncodePhrase (Encoding.UTF8, text));
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase(Encoding, string)");

			result = Encoding.ASCII.GetString (Rfc2047.EncodePhrase (Encoding.UTF8, text, 0, text.Length));
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase(Encoding, string, int, int)");

			result = Encoding.ASCII.GetString (Rfc2047.EncodeText (Encoding.UTF8, text));
			Assert.That (result, Is.EqualTo (expected), "EncodeText(Encoding, string)");

			result = Encoding.ASCII.GetString (Rfc2047.EncodeText (Encoding.UTF8, text, 0, text.Length));
			Assert.That (result, Is.EqualTo (expected), "EncodeText(Encoding, string, int, int)");
		}

		[Test]
		public void TestEncodePhraseLongSentenceWithCommas ()
		{
			const string expected = "\"Once upon a time, back when things that are old now were new, there lived a man with a very particular set of skills.\"";
			const string text = "Once upon a time, back when things that are old now were new, there lived a man with a very particular set of skills.";

			var encoded = Rfc2047.EncodePhrase (Encoding.UTF8, text);
			var result = Encoding.ASCII.GetString (encoded);
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase");

			var unquoted = MimeUtils.Unquote (result);
			Assert.That (unquoted, Is.EqualTo (text), "Unquote");
		}

		[Test]
		public void TestEncodePhraseWithInnerQuotedString ()
		{
			const string expected = "\"John \\\"Jacob Jingle Heimer\\\" Schmidt\"";
			const string text = "John \"Jacob Jingle Heimer\" Schmidt";

			var encoded = Rfc2047.EncodePhrase (Encoding.UTF8, text);
			var result = Encoding.ASCII.GetString (encoded);
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase");

			var unquoted = MimeUtils.Unquote (result);
			Assert.That (unquoted, Is.EqualTo (text), "Unquote");
		}

		[Test]
		public void TestEncodePhraseWithInnerUnicodeQuotedString1 ()
		{
			const string expected = "John =?utf-8?b?Ium7nueci0DlkI3jgYzjg4njg6HjgqTjg7MgSmFjb2IgSmluZ2xlIEhlaW1lciI=?= Schmidt";
			const string text = "John \"點看@名がドメイン Jacob Jingle Heimer\" Schmidt";

			var encoded = Rfc2047.EncodePhrase (Encoding.UTF8, text);
			var result = Encoding.ASCII.GetString (encoded);
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase");

			var decoded = Rfc2047.DecodePhrase (encoded);
			Assert.That (decoded, Is.EqualTo (text), "DecodePhrase");
		}

		[Test]
		public void TestEncodePhraseWithInnerUnicodeQuotedString2 ()
		{
			const string expected = "John =?utf-8?b?IkphY29iIEppbmdsZSDpu57nnItA5ZCN44GM44OJ44Oh44Kk44OzIEhlaW1lciI=?= Schmidt";
			const string text = "John \"Jacob Jingle 點看@名がドメイン Heimer\" Schmidt";

			var encoded = Rfc2047.EncodePhrase (Encoding.UTF8, text);
			var result = Encoding.ASCII.GetString (encoded);
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase");

			var decoded = Rfc2047.DecodePhrase (encoded);
			Assert.That (decoded, Is.EqualTo (text), "DecodePhrase");
		}

		[Test]
		public void TestEncodePhraseWithInnerUnicodeQuotedString3 ()
		{
			const string expected = "John =?utf-8?b?IkphY29iIEppbmdsZSBIZWltZXIg6bue55yLQOWQjeOBjOODieODoeOCpOODsyI=?= Schmidt";
			const string text = "John \"Jacob Jingle Heimer 點看@名がドメイン\" Schmidt";

			var encoded = Rfc2047.EncodePhrase (Encoding.UTF8, text);
			var result = Encoding.ASCII.GetString (encoded);
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase");

			var decoded = Rfc2047.DecodePhrase (encoded);
			Assert.That (decoded, Is.EqualTo (text), "DecodePhrase");
		}

		[Test]
		public void TestEncodePhraseWithInnerUnicodeQuotedString4 ()
		{
			const string expected = "John =?utf-8?q?=22Jacob_Jingle_Heimer=2C_his_name_is_my_name_too!_Whenever_he_goes_out=2C_the_?=\t=?utf-8?q?people_always_shout=2C_=5C=22There_goes_John_Jacob_Jingle_Heimer_Schmidt!=5C=22_?=\t=?utf-8?b?6bue55yLQOWQjeOBjOODieODoeOCpOODsyI=?= Schmidt";
			const string text = "John \"Jacob Jingle Heimer, his name is my name too! Whenever he goes out, the people always shout, \\\"There goes John Jacob Jingle Heimer Schmidt!\\\" 點看@名がドメイン\" Schmidt";

			var encoded = Rfc2047.EncodePhrase (Encoding.UTF8, text);
			var result = Encoding.ASCII.GetString (encoded);
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase");

			var decoded = Rfc2047.DecodePhrase (encoded);
			Assert.That (decoded, Is.EqualTo (text), "DecodePhrase");
		}

		[Test]
		public void TestEncodePhraseWithInnerUnicodeQuotedString5 ()
		{
			const string expected = "\"John \\\"Whenever he goes out, the people always shout, \\\\\\\"There goes John Jacob Jingle Heimer Schmidt!\\\\\\\"\\\" Schmidt\"";
			const string text = "John \"Whenever he goes out, the people always shout, \\\"There goes John Jacob Jingle Heimer Schmidt!\\\"\" Schmidt";

			var encoded = Rfc2047.EncodePhrase (Encoding.UTF8, text);
			var result = Encoding.ASCII.GetString (encoded);
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase");

			var unquoted = MimeUtils.Unquote (result);
			Assert.That (unquoted, Is.EqualTo (text), "Unquote");
		}

		[Test]
		public void TestEncodePhraseWithInnerUnicodeComment ()
		{
			const string expected = "\"John (Jacob Jingle Heimer) Schmidt\"";
			const string text = "John (Jacob Jingle Heimer) Schmidt";

			var encoded = Rfc2047.EncodePhrase (Encoding.UTF8, text);
			var result = Encoding.ASCII.GetString (encoded);
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase");

			var unquoted = MimeUtils.Unquote (result);
			Assert.That (unquoted, Is.EqualTo (text), "Unquote");
		}

		[Test]
		public void TestEncodePhraseWithInnerUnicodeComment1 ()
		{
			const string expected = "John =?utf-8?b?KOm7nueci0DlkI3jgYzjg4njg6HjgqTjg7MgSmFjb2IgSmluZ2xlIEhlaW1lcik=?= Schmidt";
			const string text = "John (點看@名がドメイン Jacob Jingle Heimer) Schmidt";

			var encoded = Rfc2047.EncodePhrase (Encoding.UTF8, text);
			var result = Encoding.ASCII.GetString (encoded);
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase");

			var decoded = Rfc2047.DecodePhrase (encoded);
			Assert.That (decoded, Is.EqualTo (text), "DecodePhrase");
		}

		[Test]
		public void TestEncodePhraseWithInnerUnicodeComment2 ()
		{
			const string expected = "John =?utf-8?b?KEphY29iIEppbmdsZSDpu57nnItA5ZCN44GM44OJ44Oh44Kk44OzIEhlaW1lcik=?= Schmidt";
			const string text = "John (Jacob Jingle 點看@名がドメイン Heimer) Schmidt";

			var encoded = Rfc2047.EncodePhrase (Encoding.UTF8, text);
			var result = Encoding.ASCII.GetString (encoded);
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase");

			var decoded = Rfc2047.DecodePhrase (encoded);
			Assert.That (decoded, Is.EqualTo (text), "DecodePhrase");
		}

		[Test]
		public void TestEncodePhraseWithInnerUnicodeComment3 ()
		{
			const string expected = "John =?utf-8?b?KEphY29iIEppbmdsZSBIZWltZXIg6bue55yLQOWQjeOBjOODieODoeOCpOODsyk=?= Schmidt";
			const string text = "John (Jacob Jingle Heimer 點看@名がドメイン) Schmidt";

			var encoded = Rfc2047.EncodePhrase (Encoding.UTF8, text);
			var result = Encoding.ASCII.GetString (encoded);
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase");

			var decoded = Rfc2047.DecodePhrase (encoded);
			Assert.That (decoded, Is.EqualTo (text), "DecodePhrase");
		}

		[Test]
		public void TestEncodePhraseWithInnerUnicodeComment4 ()
		{
			const string expected = "John =?utf-8?q?=28Jacob_Jingle_Heimer=2C_his_name_is_my_name_too!_Whenever_he_goes_out=2C_the_p?=\t=?utf-8?q?eople_always_shout=2C_=22There_goes_John_Jacob_Jingle_Heimer_Schmidt!=22?=\t=?utf-8?b?IOm7nueci0DlkI3jgYzjg4njg6HjgqTjg7Mp?= Schmidt";
			const string text = "John (Jacob Jingle Heimer, his name is my name too! Whenever he goes out, the people always shout, \"There goes John Jacob Jingle Heimer Schmidt!\" 點看@名がドメイン) Schmidt";

			var encoded = Rfc2047.EncodePhrase (Encoding.UTF8, text);
			var result = Encoding.ASCII.GetString (encoded);
			Assert.That (result, Is.EqualTo (expected), "EncodePhrase");

			var decoded = Rfc2047.DecodePhrase (encoded);
			Assert.That (decoded, Is.EqualTo (text), "DecodePhrase");
		}

		[Test]
		public void TestFoldMultiLineHeaderValue ()
		{
			const string expected = " This is a multi-line\r\n header value.\r\n";
			const string text = "This is a multi-line\r\nheader value.";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Dos;

			var result = Encoding.ASCII.GetString (Rfc2047.FoldUnstructuredHeader (options, "Subject", Encoding.ASCII.GetBytes (text)));
			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestFoldPreFoldedHeaderValue ()
		{
			const string expected = " This is a pre\r\n folded header value.\r\n";
			const string text = "This is a pre\r\n folded header value.";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Dos;

			var result = Encoding.ASCII.GetString (Rfc2047.FoldUnstructuredHeader (options, "Subject", Encoding.ASCII.GetBytes (text)));
			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestFoldReallyLongWordToken ()
		{
			const string expected = " This header value has a\r\n really-really-really-really-long-rfc0822-word-token-that-exceeds-the-max-allo\r\n wable-line-length-and-must-be-folded lets see what MimeKit does...\r\n";
			const string text = "This header value has a really-really-really-really-long-rfc0822-word-token-that-exceeds-the-max-allowable-line-length-and-must-be-folded lets see what MimeKit does...";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Dos;

			var result = Encoding.ASCII.GetString (Rfc2047.FoldUnstructuredHeader (options, "Subject", Encoding.ASCII.GetBytes (text)));
			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestFoldHeaderValueWithEncodedWordsIncludingLanguageCodes ()
		{
			const string expected = " I'm so happy! =?utf-8*en-US?b?8J+YgA==?= I love MIME so much\r\n =?utf-8*en-US?b?4p2k77iP4oCN8J+UpSE=?= Isn't it great?\r\n";
			const string text = "I'm so happy! =?utf-8*en-US?b?8J+YgA==?= I love MIME so much =?utf-8*en-US?b?4p2k77iP4oCN8J+UpSE=?= Isn't it great?";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Dos;

			var result = Encoding.ASCII.GetString (Rfc2047.FoldUnstructuredHeader (options, "Subject", Encoding.ASCII.GetBytes (text)));
			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestFoldHeaderValueAtTabs ()
		{
			const string expected = " I'm so happy! =?utf-8*en-US?b?8J+YgA==?= I love MIME so much\r\n\t=?utf-8*en-US?b?4p2k77iP4oCN8J+UpSE=?= Isn't it great? MIME is\r\n\tsupercalafragalisticexpialadotious, don't you think?\r\n";
			const string text = "I'm so happy! =?utf-8*en-US?b?8J+YgA==?= I love MIME so much\t=?utf-8*en-US?b?4p2k77iP4oCN8J+UpSE=?= Isn't it great? MIME is\tsupercalafragalisticexpialadotious, don't you think?";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Dos;

			var result = Encoding.ASCII.GetString (Rfc2047.FoldUnstructuredHeader (options, "Subject", Encoding.ASCII.GetBytes (text)));
			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestFoldHeaderValueWithEmbeddedEncodedWordTokens ()
		{
			const string expected = " This subject has embedded\r\n =?iso-8859-1*en-US?q?rfc2047_encoded_word_tokens?=... How does the folding\r\n logic handle these embedded=?iso-8859-1*en-US?q?rfc2047_encoded_word_tokens?=\r\n ...?\r\n";
			const string text = "This subject has embedded=?iso-8859-1*en-US?q?rfc2047_encoded_word_tokens?=... How does the folding logic handle these embedded=?iso-8859-1*en-US?q?rfc2047_encoded_word_tokens?=...?";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Dos;

			var result = Encoding.ASCII.GetString (Rfc2047.FoldUnstructuredHeader (options, "Subject", Encoding.ASCII.GetBytes (text)));
			Assert.That (result, Is.EqualTo (expected));
		}

		[Test]
		public void TestFolderHeaderValueDoesNotIgnoreWhitespaceBetweenEncodedWords ()
		{
			const string expected = " This test should demonstrate that\r\n =?iso-8859-1*en-US?q?whitespace_between_rfc2047_encoded_word_tokens?= \t \t \r\n\t =?iso-8859-1*en-US?q?does_not_get_ignored?=\r\n";
			const string text = "This test should demonstrate that =?iso-8859-1*en-US?q?whitespace_between_rfc2047_encoded_word_tokens?= \t \t \t =?iso-8859-1*en-US?q?does_not_get_ignored?=";
			var options = FormatOptions.Default.Clone ();

			options.NewLineFormat = NewLineFormat.Dos;

			var result = Encoding.ASCII.GetString (Rfc2047.FoldUnstructuredHeader (options, "Subject", Encoding.ASCII.GetBytes (text)));
			Assert.That (result, Is.EqualTo (expected));
		}
	}
}
