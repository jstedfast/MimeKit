//
// CharsetUtils.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Jeffrey Stedfast
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

namespace MimeKit {
	public static class CharsetUtils
	{
		static readonly StringComparer icase = StringComparer.InvariantCultureIgnoreCase;
		static readonly Dictionary<string, Encoding> aliases;

		static CharsetUtils ()
		{
			Encoding encoding;

			aliases = new Dictionary<string, Encoding> (icase);

			aliases.Add ("utf8", GetEncoding (65001));

			// ANSI_X3.4-1968 is used on some systems and should be
			// treated the same as US-ASCII.
			aliases.Add ("ansi_x3.4-1968", GetEncoding (20127));

			try {
				// Korean charsets
				// 'upgrade' ks_c_5601-1987 to euc-kr since it is a superset
				encoding = GetEncoding (51949); // euc-kr
				aliases.Add ("ks_c_5601-1987", encoding);
				aliases.Add ("5601",           encoding);
				aliases.Add ("ksc-5601",       encoding);
				aliases.Add ("ksc-5601-1987",  encoding);
				aliases.Add ("ksc-5601_1987",  encoding);
				aliases.Add ("ks_c_5861-1992", encoding);
				aliases.Add ("euckr-0",        encoding);
				aliases.Add ("euc-kr",         encoding);
			} catch (NotSupportedException) {
			}

			try {
				// Chinese charsets
				encoding = GetEncoding (950); // big5
				aliases.Add ("big5",           encoding);
				aliases.Add ("big5-0",         encoding);
				aliases.Add ("big5.eten-0",    encoding);
				aliases.Add ("big5hkscs-0",    encoding);
			} catch (NotSupportedException) {
			}

			try {
				// 'upgrade' gb2312 to GBK (aka euc-cn) since it is a superset
				encoding = GetEncoding (51936); // euc-cn
				aliases.Add ("gb2312",         encoding);
				aliases.Add ("gb-2312",        encoding);
				aliases.Add ("gb2312-0",       encoding);
				aliases.Add ("gb2312-80",      encoding);
				aliases.Add ("gb2312.1980-0",  encoding);
				aliases.Add ("euc-cn",         encoding);
				aliases.Add ("gbk-0",          encoding);
				aliases.Add ("gbk",            encoding);
			} catch (NotSupportedException) {
			}

			try {
				// add aliases for gb18030
				encoding = GetEncoding (54936); // gb18030
				aliases.Add ("gb18030-0",      encoding);
				aliases.Add ("gb18030",        encoding);
			} catch (NotSupportedException) {
			}

			try {
				// Japanese charsets
				encoding = GetEncoding (51932); // euc-jp
				aliases.Add ("eucjp-0",        encoding);
				aliases.Add ("euc-jp",         encoding);
				aliases.Add ("ujis-0",         encoding);
				aliases.Add ("ujis",           encoding);
			} catch (NotSupportedException) {
			}

			try {
				encoding = GetEncoding (932); // shift_jis
				aliases.Add ("jisx0208.1983-0", encoding);
				aliases.Add ("jisx0212.1990-0", encoding);
				aliases.Add ("pck",             encoding);
			} catch (NotSupportedException) {
			}

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
			Encoding encoding = GetEncoding (charset);

			if (encoding == null)
				return charset;

			return GetMimeCharset (encoding);
		}

		static int ParseIsoCodePage (string charset)
		{
			if (charset.Length < 6)
				return -1;

			int dash = charset.IndexOfAny (new char[] { '-', '_' });
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
				switch (suffix) {
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
			} else {
				foreach (var info in Encoding.GetEncodings ()) {
					if (icase.Compare (info.Name, charset) == 0)
						return info.CodePage;
				}
			}

			return -1;
		}

		public static int GetCodePage (string charset)
		{
			var encoding = GetEncoding (charset);

			return encoding != null ? encoding.CodePage : -1;
		}

		public static Encoding GetEncoding (string charset)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			Encoding encoding;

			lock (aliases) {
				if (!aliases.TryGetValue (charset, out encoding)) {
					int codepage = ParseCodePage (charset);

					if (codepage == -1)
						return null;

					encoding = GetEncoding (codepage);

					if (encoding != null) {
						aliases[encoding.HeaderName] = encoding;
						aliases[charset] = encoding;
					}
				}
			}

			return encoding;
		}

		public static Encoding GetEncoding (int codepage)
		{
			//var encoderFallback = new EncoderReplacementFallback ("?");
			//var decoderFallback = new DecoderReplacementFallback ("?");
			//
			//return Encoding.GetEncoding (codepage, encoderFallback, decoderFallback);

			return Encoding.GetEncoding (codepage);
		}

		class InvalidByteCountFallback : DecoderFallback
		{
			class InvalidByteCountFallbackBuffer : DecoderFallbackBuffer
			{
				readonly InvalidByteCountFallback fallback;
				const string replacement = "?";
				int current = 0;
				bool invalid;

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

		internal static char[] ConvertToUnicode (byte[] input, int startIndex, int length, out int charCount)
		{
			var invalid = new InvalidByteCountFallback ();
			int min = Int32.MaxValue;
			char[] output = null;
			Encoding encoding;
			Decoder decoder;
			int[] codepages;
			int best = -1;
			int count;

			// Note: 65001 is UTF-8 and 28591 is iso-8859-1
			if (Encoding.Default.CodePage != 65001 && Encoding.Default.CodePage != 28591) {
				codepages = new int[] { 65001, Encoding.Default.CodePage, 28591 };
			} else {
				codepages = new int[] { 65001, 28591 };
			}

			for (int i = 0; i < codepages.Length; i++) {
				encoding = Encoding.GetEncoding (codepages[i], new EncoderReplacementFallback ("?"), invalid);
				decoder = encoding.GetDecoder ();

				count = decoder.GetCharCount (input, startIndex, length, true);
				if (invalid.InvalidByteCount < min) {
					min = invalid.InvalidByteCount;
					best = codepages[i];

					if (min == 0)
						break;
				}

				invalid.Reset ();
			}

			encoding = CharsetUtils.GetEncoding (best);
			decoder = encoding.GetDecoder ();

			count = decoder.GetCharCount (input, startIndex, length, true);
			output = new char[count];

			charCount = decoder.GetChars (input, startIndex, length, output, 0, true);

			return output;
		}

		public static string ConvertToUnicode (byte[] buffer, int startIndex, int length)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (length < 0 || startIndex + length > buffer.Length)
				throw new ArgumentOutOfRangeException ("length");

			int count;

			return new string (ConvertToUnicode (buffer, startIndex, length, out count), 0, count);
		}

		internal static char[] ConvertToUnicode (Encoding encoding, byte[] input, int startIndex, int length, out int charCount)
		{
			var decoder = encoding.GetDecoder ();
			int count = decoder.GetCharCount (input, startIndex, length, true);
			char[] output = new char[count];

			charCount = decoder.GetChars (input, startIndex, length, output, 0, true);

			return output;
		}

		internal static char[] ConvertToUnicode (int codepage, byte[] input, int startIndex, int length, out int charCount)
		{
			Encoding encoding = null;

			if (codepage != -1)
				encoding = CharsetUtils.GetEncoding (codepage);

			if (encoding == null)
				return ConvertToUnicode (input, startIndex, length, out charCount);

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

			if (length < 0 || startIndex + length > buffer.Length)
				throw new ArgumentOutOfRangeException ("length");

			int count;

			return new string (ConvertToUnicode (encoding, buffer, startIndex, length, out count), 0, count);
		}
	}
}
