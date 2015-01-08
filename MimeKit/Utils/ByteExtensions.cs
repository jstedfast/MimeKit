//
// ByteExtensions.cs
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

namespace MimeKit.Utils {
	[Flags]
	enum CharType : ushort {
		None                  = 0,
		IsAscii               = (1 << 0),
		IsAtom                = (1 << 1),
		IsAttrChar            = (1 << 2),
		IsBlank               = (1 << 3),
		IsControl             = (1 << 4),
		IsDomainSafe          = (1 << 5),
		IsEncodedPhraseSafe   = (1 << 6),
		IsEncodedWordSafe     = (1 << 7),
		IsQuotedPrintableSafe = (1 << 8),
		IsSpace               = (1 << 9),
		IsSpecial             = (1 << 10),
		IsTokenSpecial        = (1 << 11),
		IsWhitespace          = (1 << 12),
		IsXDigit              = (1 << 13),

		IsAsciiAtom           = IsAscii | IsAtom,
	}

	static class ByteExtensions
	{
		const string AtomSafeCharacters = "!#$%&'*+-/=?^_`{|}~";
		const string AttributeSpecials = "*'%";    // attribute specials from rfc2184
		const string CommentSpecials = "()\\\r";   // not allowed in comments
		const string DomainSpecials = "[]\\\r \t"; // not allowed in domains
		const string EncodedWordSpecials = "()<>@,;:\"/[]?.=_";  // rfc2047 5.1
		const string EncodedPhraseSpecials = "!*+-/=_";          // rfc2047 5.3
		const string Specials = "()<>@,;:\\\".[]";
		internal const string TokenSpecials = "()<>@,;:\\\"/[]?=";
		const string Whitespace = " \t\r\n";

		static readonly CharType[] table = new CharType[256];

		static void RemoveFlags (string values, CharType bit)
		{
			for (int i = 0; i < values.Length; i++)
				table[(byte) values[i]] &= ~bit;
		}

		static void SetFlags (string values, CharType bit, CharType bitcopy, bool remove)
		{
			int i;

			if (remove) {
				for (i = 0; i < 128; i++)
					table[i] |= bit;

				for (i = 0; i < values.Length; i++)
					table[values[i]] &= ~bit;

				if (bitcopy != CharType.None) {
					for (i = 0; i < 256; i++) {
						if ((table[i] & bitcopy) != 0)
							table[i] &= ~bit;
					}
				}
			} else {
				for (i = 0; i < values.Length; i++)
					table[values[i]] |= bit;

				if (bitcopy != CharType.None) {
					for (i = 0; i < 256; i++) {
						if ((table[i] & bitcopy) != 0)
							table[i] |= bit;
					}
				}
			}
		}

		static ByteExtensions ()
		{
			for (int i = 0; i < 256; i++) {
				if (i < 127) {
					if (i < 32)
						table[i] |= CharType.IsControl;
					if (i > 32)
						table[i] |= CharType.IsAttrChar;
					if ((i >= 33 && i <= 60) || (i >= 62 && i <= 126) || i == 32)
						table[i] |= (CharType.IsQuotedPrintableSafe | CharType.IsEncodedWordSafe);
					if ((i >= '0' && i <= '9') || (i >= 'a' && i <= 'z') || (i >= 'A' && i <= 'Z'))
						table[i] |= CharType.IsEncodedPhraseSafe | CharType.IsAtom;
					if ((i >= '0' && i <= '9') || (i >= 'a' && i <= 'f') || (i >= 'A' && i <= 'F'))
						table[i] |= CharType.IsXDigit;

					table[i] |= CharType.IsAscii;
				} else {
					if (i == 127)
						table[i] |= CharType.IsAscii;
					else
						table[i] |= CharType.IsAtom;

					table[i] |= CharType.IsControl;
				}
			}

			table['\t'] |= CharType.IsQuotedPrintableSafe | CharType.IsBlank;
			table[' '] |= CharType.IsSpace | CharType.IsBlank;

			SetFlags (Whitespace, CharType.IsWhitespace, CharType.None, false);
			SetFlags (AtomSafeCharacters, CharType.IsAtom, CharType.None, false);
			SetFlags (TokenSpecials, CharType.IsTokenSpecial, CharType.IsControl, false);
			SetFlags (Specials, CharType.IsSpecial, CharType.None, false);
			SetFlags (DomainSpecials, CharType.IsDomainSafe, CharType.None, true);
			RemoveFlags (Specials, CharType.IsAtom);
			RemoveFlags (EncodedWordSpecials, CharType.IsEncodedWordSafe);
			RemoveFlags (AttributeSpecials + TokenSpecials, CharType.IsAttrChar);
			SetFlags (EncodedPhraseSpecials, CharType.IsEncodedPhraseSafe, CharType.None, false);
		}

		public static bool IsAscii (this byte c)
		{
			return (table[c] & CharType.IsAscii) != 0;
		}

		public static bool IsAsciiAtom (this byte c)
		{
			return (table[c] & CharType.IsAsciiAtom) == CharType.IsAsciiAtom;
		}

		public static bool IsAtom (this byte c)
		{
			return (table[c] & CharType.IsAtom) != 0;
		}

		public static bool IsAttr (this byte c)
		{
			return (table[c] & CharType.IsAttrChar) != 0;
		}

		public static bool IsBlank (this byte c)
		{
			return (table[c] & CharType.IsBlank) != 0;
		}

		public static bool IsCtrl (this byte c)
		{
			return (table[c] & CharType.IsControl) != 0;
		}

		public static bool IsDomain (this byte c)
		{
			return (table[c] & CharType.IsDomainSafe) != 0;
		}

		public static bool IsQpSafe (this byte c)
		{
			return (table[c] & CharType.IsQuotedPrintableSafe) != 0;
		}

		public static bool IsToken (this byte c)
		{
			return (table[c] & (CharType.IsTokenSpecial | CharType.IsWhitespace | CharType.IsControl)) == 0;
		}

		public static bool IsTokenSpecial (this byte c)
		{
			return (table[c] & CharType.IsTokenSpecial) != 0;
		}

		public static bool IsType (this byte c, CharType type)
		{
			return (table[c] & type) != 0;
		}

		public static bool IsWhitespace (this byte c)
		{
			return (table[c] & CharType.IsWhitespace) != 0;
		}

		public static bool IsXDigit (this byte c)
		{
			return (table[c] & CharType.IsXDigit) != 0;
		}

		public static byte ToLower (this byte c)
		{
			if (c >= 0x41 && c <= 0x5A)
				return (byte) (c + 0x20);

			return c;
		}

		public static byte ToUpper (this byte c)
		{
			if (c >= 0x61 && c <= 0x7A)
				return (byte) (c - 0x20);

			return c;
		}

		public static byte ToXDigit (this byte c)
		{
			if (c >= 0x41) {
				if (c >= 0x61)
					return (byte) (c - (0x61 - 0x0a));

				return (byte) (c - (0x41 - 0x0A));
			}

			return (byte) (c - 0x30);
		}
	}
}
