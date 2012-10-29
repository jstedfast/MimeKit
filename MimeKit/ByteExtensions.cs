//
// ByteExtensions.cs
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

namespace MimeKit {
	static class ByteExtensions
	{
		const string AttributeCharacters = "*'% "; // attribute-char from rfc2184
		const string CommentSpecials = "()\\\r";   // not allowed in comments
		const string DomainSpecials = "[]\\\r \t"; // not allowed in domains
		const string EncodedWordSpecials = "()<>@,;:\"/[]?.=_";  // rfc2047 5.1
		const string EncodedPhraseSpecials = "!*+-/=_";          // rfc2047 5.3
		const string Specials = "()<>@,;:\\\".[]";
		const string TokenSpecials = "()<>@,;:\\\"/[]?=";
		const string Whitespace = " \t\r\n";

		[Flags]
		enum CharType : ushort {
			None                  = 0,
			IsAscii               = (1 << 0),
			IsAttrChar            = (1 << 1),
			IsBlank               = (1 << 2),
			IsControl             = (1 << 3),
			IsDomainSpecial       = (1 << 4),
			IsEncodedPhraseSafe   = (1 << 5),
			IsEncodedWordSafe     = (1 << 6),
			IsQuotedPrintableSafe = (1 << 7),
			IsSpace               = (1 << 8),
			IsSpecial             = (1 << 9),
			IsTokenSpecial        = (1 << 10),
			IsWhitespace          = (1 << 11),
			IsXDigit              = (1 << 12),
		}

		static CharType[] table = new CharType[256];

		static void RemoveFlags (string values, CharType bit)
		{
			for (int i = 0; i < values.Length; i++)
				table[values[i]] &= ~bit;
		}

		static void SetFlags (string values, CharType bit, CharType bitcopy, bool remove)
		{
			int i;

			if (remove) {
				for (i = 0; i < 256; i++)
					table[i] |= bit;

				for (i = 0; i < values.Length; i++)
					table[values[i]] &= ~bit;

				if (bitcopy != CharType.None) {
					for (i = 0; i < 256; i++) {
						if (table[i].HasFlag (bitcopy))
							table[i] &= ~bit;
					}
				}
			} else {
				for (i = 0; i < values.Length; i++)
					table[values[i]] |= bit;

				if (bitcopy != CharType.None) {
					for (i = 0; i < 256; i++) {
						if (table[i].HasFlag (bitcopy))
							table[i] |= bit;
					}
				}
			}
		}

		static ByteExtensions ()
		{
			for (int i = 0; i < 256; i++) {
				table[i] = 0;
				if (i < 128) {
					if (i < 32 || i == 127)
						table[i] |= CharType.IsControl;
					if (i > 32 && i < 127)
						table[i] |= CharType.IsAttrChar;
					if ((i >= 33 && i <= 60) || (i >= 62 && i <= 126) || i == 32)
						table[i] |= (CharType.IsQuotedPrintableSafe | CharType.IsEncodedWordSafe);
					if ((i >= '0' && i <= '9') || (i >= 'a' && i <= 'z') || (i >= 'A' && i <= 'Z'))
						table[i] |= CharType.IsEncodedPhraseSafe;
					if ((i >= '0' && i <= '9') || (i >= 'a' && i <= 'f') || (i >= 'A' && i <= 'F'))
						table[i] |= CharType.IsXDigit;

					table[i] |= CharType.IsAscii;
				}
			}

			table[(int) ' '] |= CharType.IsSpace | CharType.IsBlank;
			table['\t'] |= CharType.IsQuotedPrintableSafe | CharType.IsBlank;

			SetFlags (Whitespace, CharType.IsWhitespace, CharType.None, false);
			SetFlags (TokenSpecials, CharType.IsTokenSpecial, CharType.IsControl, false);
			SetFlags (Specials, CharType.IsSpecial, CharType.None, false);
			SetFlags (DomainSpecials, CharType.IsDomainSpecial, CharType.None, false);
			RemoveFlags (EncodedWordSpecials, CharType.IsEncodedWordSafe);
			RemoveFlags (AttributeCharacters + TokenSpecials, CharType.IsAttrChar);
			SetFlags (EncodedPhraseSpecials, CharType.IsEncodedPhraseSafe, CharType.None, false);
		}

		public static bool IsAscii (this byte c)
		{
			return table[c].HasFlag (CharType.IsAscii);
		}

		public static bool IsAtom (this byte c)
		{
			const CharType NonAtomFlags = CharType.IsSpecial | CharType.IsSpace | CharType.IsControl;

			return !table[c].HasFlag (NonAtomFlags);
		}

		public static bool IsAttr (this byte c)
		{
			return table[c].HasFlag (CharType.IsAttrChar);
		}

		public static bool IsBlank (this byte c)
		{
			return table[c].HasFlag (CharType.IsBlank);
		}

		public static bool IsCtrl (this byte c)
		{
			return table[c].HasFlag (CharType.IsControl);
		}

		public static bool IsDomain (this byte c)
		{
			return !table[c].HasFlag (CharType.IsDomainSpecial);
		}

		public static bool IsQpSafe (this byte c)
		{
			return table[c].HasFlag (CharType.IsQuotedPrintableSafe);
		}

		public static bool IsXDigit (this byte c)
		{
			return table[c].HasFlag (CharType.IsXDigit);
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
					return (byte) (c - 0x61);

				return (byte) (c - 0x41);
			}

			return (byte) (c - 0x30);
		}
	}
}
