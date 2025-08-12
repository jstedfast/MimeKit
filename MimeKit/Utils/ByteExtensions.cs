﻿//
// ByteExtensions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
		IsPhraseAtom          = (1 << 14),
		IsFieldText           = (1 << 15),

		IsAsciiAtom           = IsAscii | IsAtom,
	}

	static class ByteExtensions
	{
		const string AtomSafe = "!#$%&'*+-/=?^_`{|}~";
		const string AttributeSpecials = "*'%";                    // attribute specials from rfc2184/rfc2231
		const string CommentSpecials = "()\\\r";                   // not allowed in comments
		const string EncodedWordSpecials = "()<>@,;:\"/[]?.=_";    // rfc2047 5.1
		const string EncodedPhraseSafe = "!*+-/";                  // rfc2047 5.3 (but w/o '=' and '_' since they need to be encoded, obviously)
		const string Specials = "()<>[]:;@\\,.\"";                 // rfc5322 3.2.3
		internal const string TokenSpecials = "()<>@,;:\\\"/[]?="; // rfc2045 5.1
		const string Whitespace = " \t\r\n";

		static readonly CharType[] table = new CharType[256];

		static void RemoveFlags (string values, CharType bit)
		{
			for (int i = 0; i < values.Length; i++)
				table[(byte) values[i]] &= ~bit;
		}

		static void SetFlags (string values, CharType bit)
		{
			for (int i = 0; i < values.Length; i++)
				table[values[i]] |= bit;
		}

		static ByteExtensions ()
		{
			for (int i = 0; i < 256; i++) {
				if (i < 127) {
					if (i < 32)
						table[i] |= CharType.IsControl | CharType.IsTokenSpecial;
					if (i > 32)
						table[i] |= CharType.IsAttrChar;
					if (i >= 32 && i != 61) // space + all printable characters *except* '=' (61)
						table[i] |= CharType.IsQuotedPrintableSafe | CharType.IsEncodedWordSafe;
					if ((i >= '0' && i <= '9') || (i >= 'a' && i <= 'z') || (i >= 'A' && i <= 'Z'))
						table[i] |= CharType.IsEncodedPhraseSafe | CharType.IsAtom | CharType.IsPhraseAtom;
					if ((i >= '0' && i <= '9') || (i >= 'a' && i <= 'f') || (i >= 'A' && i <= 'F'))
						table[i] |= CharType.IsXDigit;
					if (i >= 33 && i != 58) // all printable characters *except* ':'
						table[i] |= CharType.IsFieldText;
					if ((i >= 33 && i <= 90) || i >= 94) // all printable characters *except* '[', '\\', and ']'
						table[i] |= CharType.IsDomainSafe;

					table[i] |= CharType.IsAscii;
				} else {
					if (i == 127)
						table[i] |= CharType.IsAscii;
					else
						table[i] |= CharType.IsAtom | CharType.IsPhraseAtom;

					table[i] |= CharType.IsControl | CharType.IsTokenSpecial;
				}
			}

			table['\t'] |= CharType.IsQuotedPrintableSafe | CharType.IsBlank;
			table[' '] |= CharType.IsSpace | CharType.IsBlank;

			SetFlags (Whitespace, CharType.IsWhitespace);
			SetFlags (AtomSafe, CharType.IsAtom | CharType.IsPhraseAtom);
			SetFlags (TokenSpecials, CharType.IsTokenSpecial);
			SetFlags (Specials, CharType.IsSpecial);
			RemoveFlags (Specials, CharType.IsAtom | CharType.IsPhraseAtom);
			RemoveFlags (EncodedWordSpecials, CharType.IsEncodedWordSafe);
			RemoveFlags (AttributeSpecials + TokenSpecials, CharType.IsAttrChar);
			SetFlags (EncodedPhraseSafe, CharType.IsEncodedPhraseSafe);

			// Note: Allow '[' and ']' in the display-name of a mailbox address
			table['['] |= CharType.IsPhraseAtom;
			table[']'] |= CharType.IsPhraseAtom;

			// Note: Allow ')' in the display-name of a mailbox address for rfc7103 unbalanced parenthesis
			table[')'] |= CharType.IsPhraseAtom;
		}

		//public static bool IsAscii (this byte c)
		//{
		//	return (table[c] & CharType.IsAscii) != 0;
		//}

		public static bool IsAsciiAtom (this byte c)
		{
			return (table[c] & CharType.IsAsciiAtom) == CharType.IsAsciiAtom;
		}

		public static bool IsPhraseAtom (this byte c)
		{
			return (table[c] & CharType.IsPhraseAtom) != 0;
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

		public static bool IsFieldText (this byte c)
		{
			return (table[c] & CharType.IsFieldText) != 0;
		}

		public static bool IsQpSafe (this byte c)
		{
			return (table[c] & CharType.IsQuotedPrintableSafe) != 0;
		}

		public static bool IsToken (this byte c)
		{
			return (table[c] & (CharType.IsTokenSpecial | CharType.IsWhitespace | CharType.IsControl)) == 0;
		}

		//public static bool IsTokenSpecial (this byte c)
		//{
		//	return (table[c] & CharType.IsTokenSpecial) != 0;
		//}

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

		//public static byte ToLower (this byte c)
		//{
		//	if (c >= 0x41 && c <= 0x5A)
		//		return (byte) (c + 0x20);
		//
		//	return c;
		//}

		//public static byte ToUpper (this byte c)
		//{
		//	if (c >= 0x61 && c <= 0x7A)
		//		return (byte) (c - 0x20);
		//
		//	return c;
		//}

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
