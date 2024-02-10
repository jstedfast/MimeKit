//
// CharsetUtils.cs
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

using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace MimeKit.Utils {
	static class CharsetUtils
	{
		static readonly char[] DashUnderscore = new[] { '-', '_' };
		static readonly Dictionary<string, int> aliases;
		public static readonly Encoding Latin1;
		public static readonly Encoding UTF8;

		static CharsetUtils ()
		{
			try {
				Latin1 = Encoding.GetEncoding (28591, new EncoderExceptionFallback (), new DecoderExceptionFallback ());
			} catch (NotSupportedException) {
				// Note: Some ASP.NET environments running on .NET Framework >= v4.6.1 do not include the full spectrum of
				// text encodings, so iso-8859-1 will not always be available unless the program includes a reference to
				// the System.Text.Encoding.CodePages nuget package *and* it has been registered via the
				// System.Text.Encoding.RegisterProvider method. In cases where this hasn't been done, we need some fallback
				// logic that tries to handle this at least somewhat.

				// Use ASCII as a fallback.
				Latin1 = Encoding.ASCII;
			}

			// Note: Encoding.UTF8.GetString() replaces invalid bytes with a unicode '?' character,
			// so we use our own UTF8 instance when using GetString() if we do not want it to do that.
			UTF8 = Encoding.GetEncoding (65001, new EncoderExceptionFallback (), new DecoderExceptionFallback ());

			aliases = new Dictionary<string, int> (MimeUtils.OrdinalIgnoreCase);

			AddAliases (aliases, 65001, -1, "utf-8", "utf8", "unicode");

			// ANSI_X3.4-1968 is used on some systems and should be
			// treated the same as US-ASCII.
			AddAliases (aliases, 20127, -1, "ansi_x3.4-1968");

			// ANSI_X3.110-1983 is another odd-ball charset that appears
			// every once in a while and seems closest to ISO-8859-1.
			AddAliases (aliases, 28591, -1, "ansi_x3.110-1983", "latin1");

			// Macintosh aliases
			AddAliases (aliases, 10000, -1, "macintosh");
			AddAliases (aliases, 10079, -1, "x-mac-icelandic");

			// Korean charsets (aliases for euc-kr)
			// 'upgrade' ks_c_5601-1987 to euc-kr since it is a superset
			AddAliases (aliases, 51949, -1,
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
			AddAliases (aliases, 950, -1, "big5", "big5-0", "big5-hkscs", "big5.eten-0", "big5hkscs-0");

			// Chinese charsets (aliases for gb2312)
			int gb2312 = AddAliases (aliases, 936, -1, "gb2312", "gb-2312", "gb2312-0", "gb2312-80", "gb2312.1980-0");

			// Chinese charsets (euc-cn and gbk not supported on Mono)
			// https://bugzilla.mozilla.org/show_bug.cgi?id=844082 seems to suggest falling back to gb2312.
			AddAliases (aliases, 51936, gb2312, "euc-cn", "gbk-0", "x-gbk", "gbk");

			// Chinese charsets (hz-gb-2312 not suported on Mono)
			AddAliases (aliases, 52936, gb2312, "hz-gb-2312", "hz-gb2312");

			// Chinese charsets (aliases for gb18030)
			AddAliases (aliases, 54936, -1, "gb18030-0", "gb18030");

			// Japanese charsets (aliases for euc-jp)
			AddAliases (aliases, 51932, -1, "eucjp-0", "euc-jp", "ujis-0", "ujis");

			// Japanese charsets (aliases for Shift_JIS)
			AddAliases (aliases, 932, -1, "shift_jis", "jisx0208.1983-0", "jisx0212.1990-0", "pck");

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
			AddAliases (aliases, 50220, -1, "iso-2022-jp");
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

		static int AddAliases (Dictionary<string, int> dict, int codepage, int fallback, params string[] names)
		{
			int value = ProbeCharset (codepage) ? codepage : fallback;

			for (int i = 0; i < names.Length; i++)
				dict.Add (names[i], value);

			return value;
		}

		public static string GetMimeCharset (Encoding encoding)
		{
			if (encoding is null)
				throw new ArgumentNullException (nameof (encoding));

			switch (encoding.CodePage) {
			case 932:   return "shift_jis";   // shift_jis
			case 949:   return "euc-kr";      // ks_c_5601-1987
			case 50220: return "iso-2022-jp"; // csISO2022JP
			case 50221: return "iso-2022-jp"; // csISO2022JP
			case 50225: return "euc-kr";      // iso-2022-kr
			default:
				return encoding.WebName.ToLowerInvariant ();
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

		static bool TryParseInt32 (string text, int startIndex, int count, out int value)
		{
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			return int.TryParse (text.AsSpan (startIndex, count), NumberStyles.None, CultureInfo.InvariantCulture, out value);
#else
			return int.TryParse (text.Substring (startIndex, count), NumberStyles.None, CultureInfo.InvariantCulture, out value);
#endif
		}

		static int ParseIsoCodePage (string charset, int startIndex)
		{
			if ((charset.Length - startIndex) < 5)
				return -1;

			int dash = charset.IndexOfAny (DashUnderscore, startIndex);
			if (dash == -1)
				dash = charset.Length;

			if (!TryParseInt32 (charset, startIndex, dash - startIndex, out int iso))
				return -1;

			if (iso == 10646)
				return 1201;

			if (dash + 2 > charset.Length)
				return -1;

			int codepageIndex = dash + 1;
			int codepage;

			switch (iso) {
			case 8859:
				if (!TryParseInt32 (charset, codepageIndex, charset.Length - codepageIndex, out codepage))
					return -1;

				if (codepage <= 0 || (codepage > 9 && codepage < 13) || codepage > 15)
					return -1;

				codepage += 28590;
				break;
			case 2022:
				int n = charset.Length - codepageIndex;

				if (n == 2 && string.Compare (charset, codepageIndex, "jp", 0, 2, StringComparison.OrdinalIgnoreCase) == 0)
					codepage = 50220;
				else if (n == 2 && string.Compare (charset, codepageIndex, "kr", 0, 2, StringComparison.OrdinalIgnoreCase) == 0)
					codepage = 50225;
				else
					codepage = -1;
				break;
			default:
				return -1;
			}

			return codepage;
		}

		internal static int ParseCodePage (string charset)
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

				if (TryParseInt32 (charset, i, charset.Length - i, out codepage))
					return codepage;
			} else if (charset.StartsWith ("ibm", StringComparison.OrdinalIgnoreCase)) {
				i = 3;

				if (i == charset.Length)
					return -1;

				if (charset[i] == '-' || charset[i] == '_')
					i++;

				if (TryParseInt32 (charset, i, charset.Length - i, out codepage))
					return codepage;
			} else if (charset.StartsWith ("iso", StringComparison.OrdinalIgnoreCase)) {
				i = 3;

				if (i == charset.Length)
					return -1;

				if (charset[i] == '-' || charset[i] == '_')
					i++;

				if ((codepage = ParseIsoCodePage (charset, i)) != -1)
					return codepage;
			} else if (charset.StartsWith ("cp", StringComparison.OrdinalIgnoreCase)) {
				i = 2;

				if (i == charset.Length)
					return -1;

				if (charset[i] == '-' || charset[i] == '_')
					i++;

				if (TryParseInt32 (charset, i, charset.Length - i, out codepage))
					return codepage;
			} else if (charset.Equals ("latin1", StringComparison.OrdinalIgnoreCase)) {
				return 28591;
			}

			return -1;
		}

		public static int GetCodePage (string charset)
		{
			if (charset is null)
				throw new ArgumentNullException (nameof (charset));

			int codepage;

			lock (aliases) {
				if (!aliases.TryGetValue (charset, out codepage)) {
					Encoding encoding;

					codepage = ParseCodePage (charset);

					if (codepage == -1) {
						try {
							encoding = Encoding.GetEncoding (charset);
							codepage = encoding.CodePage;

							if (!aliases.ContainsKey (encoding.WebName))
								aliases[encoding.WebName] = codepage;
						} catch {
							codepage = -1;
						}
					} else {
						try {
							encoding = Encoding.GetEncoding (codepage);
							if (!aliases.ContainsKey (encoding.WebName))
								aliases[encoding.WebName] = codepage;
						} catch {
							codepage = -1;
						}
					}

					if (!aliases.ContainsKey (charset))
						aliases[charset] = codepage;
				}
			}

			return codepage;
		}

		public static Encoding GetEncoding (string charset, string fallback)
		{
			int codepage;

			if (charset is null)
				throw new ArgumentNullException (nameof (charset));

			if (fallback is null)
				throw new ArgumentNullException (nameof (fallback));

			if ((codepage = GetCodePage (charset)) == -1)
				throw new NotSupportedException ($"The '{charset}' encoding is not supported.");

			var encoderFallback = new EncoderReplacementFallback (fallback);
			var decoderFallback = new DecoderReplacementFallback (fallback);

			return Encoding.GetEncoding (codepage, encoderFallback, decoderFallback);
		}

		public static Encoding GetEncoding (string charset)
		{
			int codepage;

			if ((codepage = GetCodePage (charset)) == -1)
				throw new NotSupportedException (string.Format ("The '{0}' encoding is not supported.", charset));

			try {
				return Encoding.GetEncoding (codepage);
			} catch (Exception ex) {
				throw new NotSupportedException (string.Format ("The '{0}' encoding is not supported.", charset), ex);
			}
		}

		public static Encoding GetEncoding (int codepage, string fallback)
		{
			if (fallback is null)
				throw new ArgumentNullException (nameof (fallback));

			var encoderFallback = new EncoderReplacementFallback (fallback);
			var decoderFallback = new DecoderReplacementFallback (fallback);

			return Encoding.GetEncoding (codepage, encoderFallback, decoderFallback);
		}

		public static Encoding GetEncoding (int codepage)
		{
			return Encoding.GetEncoding (codepage);
		}

		public static Encoding GetEncodingOrDefault (int codepage, Encoding defaultEncoding)
		{
			try {
				return Encoding.GetEncoding (codepage);
			} catch {
				return defaultEncoding;
			}
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
			int min = int.MaxValue;
			int bestCharCount = 0;
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
				decoder = encoding.GetDecoder ();

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

			encoding = GetEncoding (best, "?");
			decoder = encoding.GetDecoder ();

			var output = new char[bestCharCount];

			try {
				charCount = decoder.GetChars (input, startIndex, length, output, 0, true);
			} catch {
				charCount = 0;
			}

			return output;
		}

		public static string ConvertToUnicode (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (length));

			return new string (ConvertToUnicode (options, buffer, startIndex, length, out int count), 0, count);
		}

		internal static char[] ConvertToUnicode (Encoding encoding, byte[] input, int startIndex, int length, out int charCount)
		{
			var decoder = encoding.GetDecoder ();
			int count = decoder.GetCharCount (input, startIndex, length, true);
			var output = new char[count];

			try {
				charCount = decoder.GetChars (input, startIndex, length, output, 0, true);
			} catch {
				charCount = 0;
			}

			return output;
		}

		internal static char[] ConvertToUnicode (ParserOptions options, int codepage, byte[] input, int startIndex, int length, out int charCount)
		{
			Encoding encoding = null;

			if (codepage != -1) {
				try {
					encoding = GetEncoding (codepage);
				} catch (NotSupportedException) {
				} catch (ArgumentException) {
				}
			}

			if (encoding is null)
				return ConvertToUnicode (options, input, startIndex, length, out charCount);

			return ConvertToUnicode (encoding, input, startIndex, length, out charCount);
		}

		public static string ConvertToUnicode (Encoding encoding, byte[] buffer, int startIndex, int length)
		{
			if (encoding is null)
				throw new ArgumentNullException (nameof (encoding));

			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (length));

			return new string (ConvertToUnicode (encoding, buffer, startIndex, length, out int count), 0, count);
		}

		public static bool TryGetBomEncoding (byte[] buffer, int length, out Encoding encoding)
		{
			if (length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xFE)
				encoding = Encoding.Unicode; // UTF-16LE
			else if (length >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
				encoding = Encoding.BigEndianUnicode; // UTF-16BE
			else if (length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
				encoding = UTF8; // UTF-8
			else
				encoding = null;

			return encoding != null;
		}

		public static bool TryGetBomEncoding (Stream stream, out Encoding encoding)
		{
			var bom = new byte[3];

			int n = stream.Read (bom, 0, bom.Length);

			return TryGetBomEncoding (bom, n, out encoding);
		}
	}
}
