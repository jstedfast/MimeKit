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
		static Dictionary<string, Encoding> aliases;

		static CharsetUtils ()
		{
			Encoding encoding;

			aliases = new Dictionary<string, Encoding> ();

			aliases.Add ("utf8", Encoding.GetEncoding (65001));

			// ANSI_X3.4-1968 is used on some systems and should be
			// treated the same as US-ASCII.
			aliases.Add ("ansi_x3.4-1968", Encoding.GetEncoding (20127));

			// Korean charsets
			// 'upgrade' ks_c_5601-1987 to euc-kr since it is a superset
			encoding = Encoding.GetEncoding (51949); // euc-kr
			aliases.Add ("ks_c_5601-1987", encoding);
			aliases.Add ("5601",           encoding);
			aliases.Add ("ksc-5601",       encoding);
			aliases.Add ("ksc-5601-1987",  encoding);
			aliases.Add ("ksc-5601_1987",  encoding);
			aliases.Add ("ks_c_5861-1992", encoding);
			aliases.Add ("euckr-0",        encoding);
			aliases.Add ("euc-kr",         encoding);

			// Chinese charsets
			encoding = Encoding.GetEncoding (950); // big5
			aliases.Add ("big5",           encoding);
			aliases.Add ("big5-0",         encoding);
			aliases.Add ("big5.eten-0",    encoding);
			aliases.Add ("big5hkscs-0",    encoding);

			// 'upgrade' gb2312 to GBK (aka euc-cn) since it is a superset
			encoding = Encoding.GetEncoding (51936); // euc-cn
			aliases.Add ("gb2312",         encoding);
			aliases.Add ("gb-2312",        encoding);
			aliases.Add ("gb2312-0",       encoding);
			aliases.Add ("gb2312-80",      encoding);
			aliases.Add ("gb2312.1980-0",  encoding);
			aliases.Add ("euc-cn",         encoding);
			aliases.Add ("gbk-0",          encoding);
			aliases.Add ("gbk",            encoding);

			// add aliases for gb18030
			encoding = Encoding.GetEncoding (54936); // gb18030
			aliases.Add ("gb18030-0",      encoding);
			aliases.Add ("gb18030",        encoding);

			// Japanese charsets
			encoding = Encoding.GetEncoding (51932); // euc-jp
			aliases.Add ("eucjp-0",        encoding);
			aliases.Add ("euc-jp",         encoding);
			aliases.Add ("ujis-0",         encoding);
			aliases.Add ("ujis",           encoding);

			encoding = Encoding.GetEncoding (932); // shift_jis
			aliases.Add ("jisx0208.1983-0", encoding);
			aliases.Add ("jisx0212.1990-0", encoding);
			aliases.Add ("pck",             encoding);

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

		static int GetIsoCodePage (string charset)
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

		static int GetCodePage (string charset)
		{
			int codepage;
			int i;

			if (charset.StartsWith ("windows")) {
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
			} else if (charset.StartsWith ("iso")) {
				if ((codepage = GetIsoCodePage (charset.Substring (3))) != -1)
					return codepage;
			} else if (charset.StartsWith ("cp")) {
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

		public static Encoding GetEncoding (string charset)
		{
			if (charset == null)
				throw new ArgumentNullException ("charset");

			charset = charset.ToLowerInvariant ();

			Encoding encoding;

			lock (aliases) {
				if (!aliases.TryGetValue (charset, out encoding)) {
					int codepage = GetCodePage (charset);

					if (codepage == -1)
						return null;

					encoding = Encoding.GetEncoding (codepage);
					aliases.Add (encoding.HeaderName, encoding);
				}
			}

			return encoding;
		}
	}
}
