//
// CharsetUtils.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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
using System.Collections.Generic;

#if PORTABLE
using EncoderReplacementFallback = Portable.Text.EncoderReplacementFallback;
using DecoderReplacementFallback = Portable.Text.DecoderReplacementFallback;
using EncoderExceptionFallback = Portable.Text.EncoderExceptionFallback;
using DecoderExceptionFallback = Portable.Text.DecoderExceptionFallback;
using EncoderFallbackException = Portable.Text.EncoderFallbackException;
using DecoderFallbackException = Portable.Text.DecoderFallbackException;
using DecoderFallbackBuffer = Portable.Text.DecoderFallbackBuffer;
using DecoderFallback = Portable.Text.DecoderFallback;
using Encoding = Portable.Text.Encoding;
using Encoder = Portable.Text.Encoder;
using Decoder = Portable.Text.Decoder;
#endif

namespace MimeKit.Utils {
	static class CharsetUtils
	{
		// Note: Encoding.UTF8.GetString() replaces invalid bytes with a unicode '?' character,
		// so we use our own UTF8 instance when using GetString() if we do not want it to do that.
		public static readonly Encoding Latin1 = Encoding.GetEncoding (28591, new EncoderExceptionFallback (), new DecoderExceptionFallback ());
		public static readonly Encoding UTF8 = Encoding.GetEncoding (65001, new EncoderExceptionFallback (), new DecoderExceptionFallback ());
		static readonly Dictionary<string, int> aliases = new Dictionary<string, int> (StringComparer.OrdinalIgnoreCase);

		static CharsetUtils ()
		{
			int gb2312;

			AddAliases (65001, -1, "utf-8", "utf8");

			// ANSI_X3.4-1968 is used on some systems and should be
			// treated the same as US-ASCII.
			AddAliases (20127, -1, "ansi_x3.4-1968");

			// ANSI_X3.110-1983 is another odd-ball charset that appears
			// every once in a while and seems closest to ISO-8859-1.
			AddAliases (28591, -1, "ansi_x3.110-1983", "latin1");

			// Macintosh aliases
			AddAliases (10000, -1, "macintosh");
			AddAliases (10079, -1, "x-mac-icelandic");

			// Korean charsets (aliases for euc-kr)
			// 'upgrade' ks_c_5601-1987 to euc-kr since it is a superset
			AddAliases (51949, -1,
				"ks_c_5601-1987",
				"ksc-5601-1987",
				"ksc-5601_1987",
				"ksc-5601",
				"5601",
				"ks_c_5861-1992",
				"ksc-5861-1992",
				"ksc-5861_1992",
				"euckr-0",
				"euc-kr");

			// Chinese charsets (aliases for big5)
			AddAliases (950, -1, "big5", "big5-0", "big5-hkscs", "big5.eten-0", "big5hkscs-0");

			// Chinese charsets (aliases for gb2312)
			gb2312 = AddAliases (936, -1, "gb2312", "gb-2312", "gb2312-0", "gb2312-80", "gb2312.1980-0");

			// Chinese charsets (euc-cn and gbk not supported on Mono)
			// https://bugzilla.mozilla.org/show_bug.cgi?id=844082 seems to suggest falling back to gb2312.
			AddAliases (51936, gb2312, "euc-cn", "gbk-0", "x-gbk", "gbk");

			// Chinese charsets (hz-gb-2312 not suported on Mono)
			AddAliases (52936, gb2312, "hz-gb-2312", "hz-gb2312");

			// Chinese charsets (aliases for gb18030)
			AddAliases (54936, -1, "gb18030-0", "gb18030");

			// Japanese charsets (aliases for euc-jp)
			AddAliases (51932, -1, "eucjp-0", "euc-jp", "ujis-0", "ujis");

			// Japanese charsets (aliases for Shift_JIS)
			AddAliases (932, -1, "jisx0208.1983-0", "jisx0212.1990-0", "pck");

			// Note from http://msdn.microsoft.com/en-us/library/system.text.encoding.getencodings.aspx
			// Encodings 50220 and 50222 are both associated with the name "iso-2022-jp", but they
			// are not identical. Encoding 50220 converts half-width Katakana characters to
			// full-width Katakana characters, whereas encoding 50222 uses a shift-in/shift-out
			// sequence to encode half-width Katakana characters. The display name for encoding
			// 50222 is "Japanese (JIS-Allow 1 byte Kana - SO/SI)" to distinguish it from encoding
			// 50220, which has the display name "Japanese (JIS)".
			//
			// If your application requests the encoding name "iso-2022-jp", the .NET Framework
			// returns encoding 50220. However, the encoding that is appropriate for your application
			// will depend on the preferred treatment of the half-width Katakana characters.
		}

		static bool ProbeCharset (int codepage)
		{
			try {
				Encoding.GetEncoding (codepage);
				return true;
			} catch {
				return false;
			}
		}

		static int AddAliases (int codepage, int fallback, params string[] names)
		{
			int value = ProbeCharset (codepage) ? codepage : fallback;

			for (int i = 0; i < names.Length; i++)
				aliases.Add (names[i], value);

			return value;
		}

		public static string GetMimeCharset (Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			switch (encoding.CodePage) {
			case 949: // ks_c_5601-1987
				return "euc-kr";
			default:
				return encoding.HeaderName.ToLowerInvariant ();
			}
		}

		public static string GetMimeCharset (string charset)
		{
			try {
				var encoding = GetEncoding (charset);
				return GetMimeCharset (encoding);
			} catch (NotSupportedException) {
				return charset;
			}
		}

		static int ParseIsoCodePage (string charset)
		{
			if (charset.Length < 6)
				return -1;

			int dash = charset.IndexOfAny (new [] { '-', '_' });
			if (dash == -1)
				dash = charset.Length;

			int iso;
			if (!int.TryParse (charset.Substring (0, dash), out iso))
				return -1;

			if (iso == 10646)
				return 1201;

			if (dash + 2 > charset.Length)
				return -1;

			string suffix = charset.Substring (dash + 1);
			int codepage;

			switch (iso) {
			case 8859:
				if (!int.TryParse (suffix, out codepage))
					return -1;

				if (codepage <= 0 || (codepage > 9 && codepage < 13) || codepage > 15)
					return -1;

				codepage += 28590;
				break;
			case 2022:
				switch (suffix.ToLowerInvariant ()) {
				case "jp":
					codepage = 50220;
					break;
				case "kr":
					codepage = 50225;
					break;
				default:
					return -1;
				}
				break;
			default:
				return -1;
			}

			return codepage;
		}

		static int ParseCodePage (string charset)
		{
			int codepage;
			int i;

			if (charset.StartsWith ("windows", StringComparison.OrdinalIgnoreCase)) {
				i = 7;

				if (i == charset.Length)
					return -1;

				if (charset[i] == '-' || charset[i] == '_') {
					if (i + 1 == charset.Length)
						return -1;

					i++;
				}

				if (i + 2 < charset.Length && charset[i] == 'c' && charset[i + 1] == 'p')
					i += 2;

				if (int.TryParse (charset.Substring (i), out codepage))
					return codepage;
			} else if (charset.StartsWith ("ibm", StringComparison.OrdinalIgnoreCase)) {
				i = 3;

				if (i == charset.Length)
					return -1;

				if (charset[i] == '-' || charset[i] == '_')
					i++;

				if (int.TryParse (charset.Substring (i), out codepage))
					return codepage;
			} else if (charset.StartsWith ("iso", StringComparison.OrdinalIgnoreCase)) {
				i = 3;

				if (i == charset.Length)
					return -1;

				if (charset[i] == '-' || charset[i] == '_')
					i++;

				if ((codepage = ParseIsoCodePage (charset.Substring (i))) != -1)
					return codepage;
			} else if (charset.StartsWith ("cp", StringComparison.OrdinalIgnoreCase)) {
				i = 2;

				if (i == charset.Length)
					return -1;

				if (charset[i] == '-' || charset[i] == '_')
					i++;

				if (int.TryParse (charset.Substring (i), out codepage))
					return codepage;
			} else if (charset == "latin1") {
				return 28591;
			}

			return -1;
		}

		public static int GetCodePage (string charset)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			int codepage;

			lock (aliases) {
				if (!aliases.TryGetValue (charset, out codepage)) {
					Encoding encoding;

					codepage = ParseCodePage (charset);

					if (codepage == -1) {
						try {
							encoding = Encoding.GetEncoding (charset);
							codepage = encoding.CodePage;

							aliases[encoding.HeaderName] = codepage;
						} catch {
							codepage = -1;
						}
					} else {
						try {
							encoding = Encoding.GetEncoding (codepage);
							aliases[encoding.HeaderName] = codepage;
						} catch {
							codepage = -1;
						}
					}

					aliases[charset] = codepage;
				}
			}

			return codepage;
		}

		public static Encoding GetEncoding (string charset, string fallback)
		{
			int codepage;

			if (charset == null)
				throw new ArgumentNullException ("charset");

			if (fallback == null)
				throw new ArgumentNullException ("fallback");

			if ((codepage = GetCodePage (charset)) == -1)
				throw new NotSupportedException ();

			var encoderFallback = new EncoderReplacementFallback (fallback);
			var decoderFallback = new DecoderReplacementFallback (fallback);

			return Encoding.GetEncoding (codepage, encoderFallback, decoderFallback);
		}

		public static Encoding GetEncoding (string charset)
		{
			int codepage = GetCodePage (charset);

			if (codepage == -1)
				throw new NotSupportedException ();

			return Encoding.GetEncoding (codepage);
		}

		public static Encoding GetEncoding (int codepage, string fallback)
		{
			if (fallback == null)
				throw new ArgumentNullException ("fallback");

			var encoderFallback = new EncoderReplacementFallback (fallback);
			var decoderFallback = new DecoderReplacementFallback (fallback);

			return Encoding.GetEncoding (codepage, encoderFallback, decoderFallback);
		}

		public static Encoding GetEncoding (int codepage)
		{
			return Encoding.GetEncoding (codepage);
		}

		class InvalidByteCountFallback : DecoderFallback
		{
			class InvalidByteCountFallbackBuffer : DecoderFallbackBuffer
			{
				readonly InvalidByteCountFallback fallback;
				const string replacement = "?";
				bool invalid;
				int current;

				public InvalidByteCountFallbackBuffer (InvalidByteCountFallback fallback)
				{
					this.fallback = fallback;
				}

				public override bool Fallback (byte[] bytesUnknown, int index)
				{
					fallback.InvalidByteCount++;
					invalid = true;
					current = 0;
					return true;
				}

				public override char GetNextChar ()
				{
					if (!invalid)
						return '\0';

					if (current == replacement.Length)
						return '\0';

					return replacement[current++];
				}

				public override bool MovePrevious ()
				{
					if (current == 0)
						return false;

					current--;

					return true;
				}

				public override int Remaining {
					get { return invalid ? replacement.Length - current : 0; }
				}

				public override void Reset ()
				{
					invalid = false;
					current = 0;

					base.Reset ();
				}
			}

			public InvalidByteCountFallback ()
			{
				Reset ();
			}

			public int InvalidByteCount {
				get; private set;
			}

			public void Reset ()
			{
				InvalidByteCount = 0;
			}

			public override DecoderFallbackBuffer CreateFallbackBuffer ()
			{
				return new InvalidByteCountFallbackBuffer (this);
			}

			public override int MaxCharCount {
				get { return 1; }
			}
		}

		internal static char[] ConvertToUnicode (ParserOptions options, byte[] input, int startIndex, int length, out int charCount)
		{
			var invalid = new InvalidByteCountFallback ();
			var userCharset = options.CharsetEncoding;
			int min = Int32.MaxValue;
			int bestCharCount = 0;
			char[] output = null;
			Encoding encoding;
			Decoder decoder;
			int[] codepages;
			int best = -1;
			int count;

			// Note: 65001 is UTF-8 and 28591 is iso-8859-1
			if (userCharset != null && userCharset.CodePage != 65001 && userCharset.CodePage != 28591) {
				codepages = new [] { 65001, userCharset.CodePage, 28591 };
			} else {
				codepages = new [] { 65001, 28591 };
			}

			for (int i = 0; i < codepages.Length; i++) {
				encoding = Encoding.GetEncoding (codepages[i], new EncoderReplacementFallback ("?"), invalid);
				decoder = (Decoder) encoding.GetDecoder ();

				count = decoder.GetCharCount (input, startIndex, length, true);
				if (invalid.InvalidByteCount < min) {
					min = invalid.InvalidByteCount;
					bestCharCount = count;
					best = codepages[i];

					if (min == 0)
						break;
				}

				invalid.Reset ();
			}

			encoding = CharsetUtils.GetEncoding (best, "?");
			decoder = (Decoder) encoding.GetDecoder ();
			output = new char[bestCharCount];

			charCount = decoder.GetChars (input, startIndex, length, output, 0, true);

			return output;
		}

		public static string ConvertToUnicode (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException ("length");

			int count;

			return new string (ConvertToUnicode (options, buffer, startIndex, length, out count), 0, count);
		}

		internal static char[] ConvertToUnicode (Encoding encoding, byte[] input, int startIndex, int length, out int charCount)
		{
			var decoder = encoding.GetDecoder ();
			int count = decoder.GetCharCount (input, startIndex, length, true);
			var output = new char[count];

			charCount = decoder.GetChars (input, startIndex, length, output, 0, true);

			return output;
		}

		internal static char[] ConvertToUnicode (ParserOptions options, int codepage, byte[] input, int startIndex, int length, out int charCount)
		{
			Encoding encoding = null;

			if (codepage != -1) {
				try {
					encoding = CharsetUtils.GetEncoding (codepage);
				} catch (NotSupportedException) {
				}
			}

			if (encoding == null)
				return ConvertToUnicode (options, input, startIndex, length, out charCount);

			return ConvertToUnicode (encoding, input, startIndex, length, out charCount);
		}

		public static string ConvertToUnicode (Encoding encoding, byte[] buffer, int startIndex, int length)
		{
			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException ("length");

			int count;

			return new string (ConvertToUnicode (encoding, buffer, startIndex, length, out count), 0, count);
		}
	}
}
