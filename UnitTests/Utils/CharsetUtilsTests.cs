//
// CharsetUtilsTests.cs
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

using MimeKit;
using MimeKit.Utils;

namespace UnitTests.Utils {
	[TestFixture]
	public class CharsetUtilsTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var buffer = new byte[10];

			Assert.Throws<ArgumentNullException> (() => CharsetUtils.ConvertToUnicode ((ParserOptions) null, buffer, 0, buffer.Length));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.ConvertToUnicode (ParserOptions.Default, null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => CharsetUtils.ConvertToUnicode (ParserOptions.Default, buffer, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => CharsetUtils.ConvertToUnicode (ParserOptions.Default, buffer, 0, -1));

			Assert.Throws<ArgumentNullException> (() => CharsetUtils.ConvertToUnicode ((Encoding) null, buffer, 0, buffer.Length));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.ConvertToUnicode (Encoding.UTF8, null, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => CharsetUtils.ConvertToUnicode (Encoding.UTF8, buffer, -1, 0));
			Assert.Throws<ArgumentOutOfRangeException> (() => CharsetUtils.ConvertToUnicode (Encoding.UTF8, buffer, 0, -1));

			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetCodePage (null));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetEncoding (null));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetEncoding (null, "fallback"));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetEncoding ("charset", null));
			Assert.Throws<ArgumentOutOfRangeException> (() => CharsetUtils.GetEncoding (-1, "fallback"));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetEncoding (28591, null));

			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetMimeCharset ((string) null));
			Assert.Throws<ArgumentNullException> (() => CharsetUtils.GetMimeCharset ((Encoding) null));
		}

		[Test]
		public void TestNotSupportedExceptions ()
		{
			Assert.Throws<NotSupportedException> (() => CharsetUtils.GetEncoding ("x-undefined"));
			Assert.Throws<NotSupportedException> (() => CharsetUtils.GetEncoding ("x-undefined", "?"));
		}

		[Test]
		public void TestParseCodePage ()
		{
			Assert.That (CharsetUtils.ParseCodePage ("iso10646"), Is.EqualTo (1201));
			Assert.That (CharsetUtils.ParseCodePage ("iso-10646"), Is.EqualTo (1201));
			Assert.That (CharsetUtils.ParseCodePage ("iso10646-1"), Is.EqualTo (1201));
			Assert.That (CharsetUtils.ParseCodePage ("iso-10646-1"), Is.EqualTo (1201));

			Assert.That (CharsetUtils.ParseCodePage ("iso8859-1"), Is.EqualTo (28591));
			Assert.That (CharsetUtils.ParseCodePage ("iso8859_1"), Is.EqualTo (28591));
			Assert.That (CharsetUtils.ParseCodePage ("iso-8859-1"), Is.EqualTo (28591));
			Assert.That (CharsetUtils.ParseCodePage ("iso_8859_1"), Is.EqualTo (28591));
			Assert.That (CharsetUtils.ParseCodePage ("latin1"), Is.EqualTo (28591));

			Assert.That (CharsetUtils.ParseCodePage ("iso2022-jp"), Is.EqualTo (50220));
			Assert.That (CharsetUtils.ParseCodePage ("iso-2022-jp"), Is.EqualTo (50220));
			Assert.That (CharsetUtils.ParseCodePage ("iso_2022_jp"), Is.EqualTo (50220));
			Assert.That (CharsetUtils.ParseCodePage ("iso2022-kr"), Is.EqualTo (50225));
			Assert.That (CharsetUtils.ParseCodePage ("iso-2022-kr"), Is.EqualTo (50225));
			Assert.That (CharsetUtils.ParseCodePage ("iso_2022_kr"), Is.EqualTo (50225));

			Assert.That (CharsetUtils.ParseCodePage ("windows-cp1252"), Is.EqualTo (1252));
			Assert.That (CharsetUtils.ParseCodePage ("windows-1252"), Is.EqualTo (1252));
			Assert.That (CharsetUtils.ParseCodePage ("cp-1252"), Is.EqualTo (1252));
			Assert.That (CharsetUtils.ParseCodePage ("cp1252"), Is.EqualTo (1252));

			Assert.That (CharsetUtils.ParseCodePage ("cp"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("iso"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("ibm"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("windows"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("windows-"), Is.EqualTo (-1));

			Assert.That (CharsetUtils.ParseCodePage ("iso-8859"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("iso-BB59"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("iso-8859-"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("iso-8859-A"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("iso-2022-US"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("iso-4999-1"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("iso-abcd-1"), Is.EqualTo (-1));
		}

		[Test]
		public void TestConvertToUnicode ()
		{
			const string expected = "要連両存使破薫聞載線弁明小設幸名代開告覧。胸問中写視映的収掲笛事来更知。会教握話整団負画断問囲士英高枠営言。近着開交識営害新緑提犯趣大第快者一代田。海補間山間府序打面真教義優決位。需著会容同警士趣場社主交干今兆。電増罪合三時株再姿県人生。広暮分昭熊勢戦帯票昨切議読権月期春著。生上呼警上際岡作朝米趙情。";
			var options = ParserOptions.Default.Clone ();
			var gb2312 = Encoding.GetEncoding (936);

			options.CharsetEncoding = gb2312;

			var input = gb2312.GetBytes (expected);

			var actual = CharsetUtils.ConvertToUnicode (options, input, 0, input.Length);
			Assert.That (actual, Is.EqualTo (expected), "ConvertToUnicode(ParserOptions,byte[],int,int)");

			actual = CharsetUtils.ConvertToUnicode (gb2312, input, 0, input.Length);
			Assert.That (actual, Is.EqualTo (expected), "ConvertToUnicode(Encoding,byte[],int,int)");
		}

		[Test]
		public void TestGetMimeCharset ()
		{
			Encoding encoding;

			Assert.That (CharsetUtils.GetMimeCharset ("latin1"), Is.EqualTo ("iso-8859-1"));
			Assert.That (CharsetUtils.GetMimeCharset (CharsetUtils.Latin1), Is.EqualTo ("iso-8859-1"));
			Assert.That (CharsetUtils.GetMimeCharset ("gibberish"), Is.EqualTo ("gibberish"));

			Assert.That (CharsetUtils.GetMimeCharset (Encoding.GetEncoding (932)), Is.EqualTo ("shift_jis"));
			Assert.That (CharsetUtils.GetMimeCharset (Encoding.GetEncoding (50220)), Is.EqualTo ("iso-2022-jp"));
			Assert.That (CharsetUtils.GetMimeCharset (Encoding.GetEncoding (50221)), Is.EqualTo ("iso-2022-jp"));
			Assert.That (CharsetUtils.GetMimeCharset (Encoding.GetEncoding (50222)), Is.EqualTo ("iso-2022-jp"));

			try {
				encoding = Encoding.GetEncoding (50225);
			} catch (NotSupportedException) {
				encoding = null;
			}

			if (encoding != null)
				Assert.That (CharsetUtils.GetMimeCharset (encoding), Is.EqualTo ("euc-kr"));

			Assert.That (CharsetUtils.GetMimeCharset (Encoding.GetEncoding (949)), Is.EqualTo ("euc-kr"));
		}

		[TestCase ("")]
		[TestCase ("plain ASCII")]
		[TestCase ("Ren\u00e9e M\u00fcller")]
		[TestCase ("\u674e\u5c0f\u9f8d")]
		[TestCase ("\0\u007f\u0080\u07ff\u0800\ud7ff\ue000\uffff\U00010000\U0010ffff")]
		public void TestConvertToUnicodePrefersValidUtf8 (string expected)
		{
			var bytes = Encoding.UTF8.GetBytes (expected);
			var input = new byte[bytes.Length + 2];

			input[0] = input[input.Length - 1] = 0xff;
			bytes.CopyTo (input, 1);

			foreach (int codepage in new[] { 65001, 28591, 1252, 936, 932 }) {
				var options = new ParserOptions { CharsetEncoding = Encoding.GetEncoding (codepage) };
				var output = CharsetUtils.ConvertToUnicode (options, input, 1, bytes.Length, out int count);

				Assert.That (count, Is.EqualTo (expected.Length));
				Assert.That (new string (output, 0, count), Is.EqualTo (expected));
			}
		}

		static long MeasureUtf8ConversionAllocation (byte[] input, bool guessCharset)
		{
			long before = GC.GetAllocatedBytesForCurrentThread ();

			for (int i = 0; i < 10; i++) {
				if (guessCharset)
					CharsetUtils.ConvertToUnicode (ParserOptions.Default, input, 0, input.Length, out _);
				else
					Encoding.UTF8.GetChars (input, 0, input.Length);
			}

			return GC.GetAllocatedBytesForCurrentThread () - before;
		}

		[Test]
		public void TestConvertValidUtf8AllocatesOnlyOutput ()
		{
			if (TestHelper.IsCodeCoverageEnabled)
				Assert.Ignore ("The code-coverage recorder allocates memory, so allocation budgets cannot be measured.");

			var input = Encoding.UTF8.GetBytes ("Ren\u00e9e M\u00fcller");

			MeasureUtf8ConversionAllocation (input, true);
			MeasureUtf8ConversionAllocation (input, false);

			Assert.That (MeasureUtf8ConversionAllocation (input, true), Is.EqualTo (MeasureUtf8ConversionAllocation (input, false)));
		}

		[Test]
		public void TestAppendValidUtf8DoesNotAllocate ()
		{
			if (TestHelper.IsCodeCoverageEnabled)
				Assert.Ignore ("The code-coverage recorder allocates memory, so allocation budgets cannot be measured.");

			var input = Encoding.UTF8.GetBytes ("Ren\u00e9e M\u00fcller \U0001f600");
			var output = new ValueStringBuilder (input.Length * 101);

			try {
				CharsetUtils.ConvertToUnicode (ParserOptions.Default, input, 0, input.Length, ref output);
				long before = GC.GetAllocatedBytesForCurrentThread ();

				for (int i = 0; i < 100; i++)
					CharsetUtils.ConvertToUnicode (ParserOptions.Default, input, 0, input.Length, ref output);

				Assert.That (GC.GetAllocatedBytesForCurrentThread () - before, Is.EqualTo (0));
			} finally {
				output.Dispose ();
			}
		}

		[TestCase (new byte[] { 0xc3 }, 28591, "\u00c3")]
		[TestCase (new byte[] { 0xc0, 0xaf }, 28591, "\u00c0\u00af")]
		[TestCase (new byte[] { 0xed, 0xa0, 0x80 }, 28591, "\u00ed\u00a0\u0080")]
		[TestCase (new byte[] { 0xf4, 0x90, 0x80, 0x80 }, 28591, "\u00f4\u0090\u0080\u0080")]
		[TestCase (new byte[] { 0x80 }, 1252, "\u20ac")]
		[TestCase (new byte[] { 0xc3, 0xa9, 0xff }, 28591, "\u00c3\u00a9\u00ff")]
		public void TestConvertInvalidUtf8UsesCharsetFallback (byte[] input, int codepage, string expected)
		{
			var options = new ParserOptions { CharsetEncoding = Encoding.GetEncoding (codepage) };

			Assert.That (CharsetUtils.ConvertToUnicode (options, input, 0, input.Length), Is.EqualTo (expected));
		}

		[Test]
		public void TestParseInvalidCodePageNames ()
		{
			Assert.That (CharsetUtils.ParseCodePage ("cp-"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("cp125x"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("ibm-"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("ibm437x"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("windows_"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.ParseCodePage ("windows-125x"), Is.EqualTo (-1));
		}

		[Test]
		public void TestParseCodePageAliases ()
		{
			Assert.That (CharsetUtils.ParseCodePage ("ibm437"), Is.EqualTo (437));
			Assert.That (CharsetUtils.ParseCodePage ("ibm-437"), Is.EqualTo (437));
			Assert.That (CharsetUtils.ParseCodePage ("windowscp1252"), Is.EqualTo (1252));
		}

		[Test]
		public void TestParseCodePageIsCaseInsensitive ()
		{
			Assert.That (CharsetUtils.ParseCodePage ("WINDOWS-CP1252"), Is.EqualTo (1252));
			Assert.That (CharsetUtils.ParseCodePage ("Windows-Cp1252"), Is.EqualTo (1252));
			Assert.That (CharsetUtils.ParseCodePage ("windows_cP1252"), Is.EqualTo (1252));
			Assert.That (CharsetUtils.ParseCodePage ("WINDOWSCP1252"), Is.EqualTo (1252));
			Assert.That (CharsetUtils.ParseCodePage ("IBM-437"), Is.EqualTo (437));
			Assert.That (CharsetUtils.ParseCodePage ("CP-1252"), Is.EqualTo (1252));
			Assert.That (CharsetUtils.ParseCodePage ("ISO-8859-1"), Is.EqualTo (28591));
			Assert.That (CharsetUtils.ParseCodePage ("ISO-2022-JP"), Is.EqualTo (50220));
			Assert.That (CharsetUtils.ParseCodePage ("LATIN1"), Is.EqualTo (28591));
		}

		[Test]
		public void TestGetCodePageUnsupportedCharsets ()
		{
			Assert.That (CharsetUtils.GetCodePage ("x-undefined"), Is.EqualTo (-1));
			Assert.That (CharsetUtils.GetCodePage ("cp999999"), Is.EqualTo (-1));
		}

		[Test]
		public void TestGetEncodingWithReplacementFallback ()
		{
			var encoding = CharsetUtils.GetEncoding ("us-ascii", "_");

			Assert.That (encoding.GetString (new byte[] { 0x41, 0xff, 0x42 }), Is.EqualTo ("A_B"));
			Assert.That (encoding.GetString (encoding.GetBytes ("A\u0100B")), Is.EqualTo ("A_B"));
		}

		[Test]
		public void TestGetEncodingOrDefault ()
		{
			Assert.That (CharsetUtils.GetEncodingOrDefault (65001, Encoding.ASCII).CodePage, Is.EqualTo (65001));
			Assert.That (CharsetUtils.GetEncodingOrDefault (999999, Encoding.ASCII), Is.SameAs (Encoding.ASCII));
		}

		[Test]
		public void TestConvertToUnicodeFallsBackToLatin1 ()
		{
			var input = new byte[] { 0x48, 0x65, 0xc3, 0x28, 0x6f };

			Assert.That (CharsetUtils.ConvertToUnicode (ParserOptions.Default, input, 0, input.Length), Is.EqualTo ("HeÃ(o"));
		}

		[Test]
		public void TestConvertToUnicodeWithUnsupportedCodePageFallsBackToBestEncoding ()
		{
			var input = Encoding.UTF8.GetBytes ("hello");
			var output = new ValueStringBuilder (1);

			CharsetUtils.ConvertToUnicode (ParserOptions.Default, 999999, input, 0, input.Length, ref output);

			Assert.That (output.Length, Is.EqualTo (5));
			Assert.That (output.ToString (), Is.EqualTo ("hello"));
		}

		[Test]
		public void TestConvertToUnicodeReturnsEmptyStringWhenDecoderGetCharsThrows ()
		{
			var input = new byte[] { 0x41 };

			Assert.That (CharsetUtils.ConvertToUnicode (new ThrowingGetCharsEncoding (), input, 0, input.Length), Is.EqualTo (string.Empty));
		}

		class ThrowingGetCharsEncoding : Encoding
		{
			public override int GetByteCount (char[] chars, int index, int count)
			{
				return count;
			}

			public override int GetBytes (char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
			{
				for (int i = 0; i < charCount; i++)
					bytes[byteIndex + i] = (byte) chars[charIndex + i];

				return charCount;
			}

			public override int GetCharCount (byte[] bytes, int index, int count)
			{
				return count;
			}

			public override int GetChars (byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
			{
				throw new DecoderFallbackException ("GetChars failed.");
			}

			public override Decoder GetDecoder ()
			{
				return new ThrowingGetCharsDecoder ();
			}

			public override int GetMaxByteCount (int charCount)
			{
				return charCount;
			}

			public override int GetMaxCharCount (int byteCount)
			{
				return byteCount;
			}
		}

		class ThrowingGetCharsDecoder : Decoder
		{
			public override int GetCharCount (byte[] bytes, int index, int count)
			{
				return count;
			}

			public override int GetChars (byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
			{
				throw new DecoderFallbackException ("GetChars failed.");
			}
		}
	}
}
