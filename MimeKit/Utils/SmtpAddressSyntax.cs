//
// SmtpAddressSyntax.cs
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

using System;

namespace MimeKit.Utils {
	/// <summary>
	/// The RFC 5321 address syntax shared by <see cref="SmtpAddressValidator"/> and <see cref="SmtpUtf8AddressValidator"/>.
	/// </summary>
	/// <remarks>
	/// The grammar accepts UTF8-non-ascii (as extended by RFC 6531) so that <see cref="SmtpAddressValidator"/> can
	/// distinguish an address that requires SMTPUTF8 from one that is malformed.
	/// </remarks>
	static class SmtpAddressSyntax
	{
		const char Escape = (char) 0x1B;

		enum Iso2022JpMode
		{
			Ascii,
			JisRoman,
			JisX0208
		}

		// RFC 5321, Section 4.1.2:
		//
		// Mailbox = Local-part "@" ( Domain / address-literal )
		//
		// RFC 6531 extends atext, qtextSMTP and sub-domain to allow UTF8-non-ascii.
		internal static bool ValidateSyntax (string address, bool allowIso2022Jp, out bool localPartALabel, out bool domainALabel, out int domainIndex)
		{
			int index = 0;

			localPartALabel = false;
			domainALabel = false;
			domainIndex = -1;

			if (address.Length > 0 && address[0] == '"') {
				if (allowIso2022Jp ? !SkipIso2022JpQuotedString (address, ref index) : !SkipQuotedString (address, ref index))
					return false;
			} else {
				if (!SkipDotString (address, ref index, ref localPartALabel))
					return false;
			}

			if (index >= address.Length || address[index] != '@')
				return false;

			domainIndex = ++index;

			if (index < address.Length && address[index] == '[') {
				if (!SkipAddressLiteral (address, ref index))
					return false;
			} else {
				if (!SkipDomain (address, ref index, ref domainALabel))
					return false;
			}

			return index == address.Length;
		}

		internal static bool IsAsciiLetter (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
		}

		internal static bool IsAsciiLetterOrDigit (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
		}

		internal static bool IsHexDigit (char c)
		{
			return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
		}

		// RFC 5322: atext = ALPHA / DIGIT / "!" / "#" / "$" / "%" / "&" / "'" / "*" / "+" / "-" / "/" / "=" / "?" / "^" / "_" / "`" / "{" / "|" / "}" / "~"
		// RFC 6531: atext =/ UTF8-non-ascii
		internal static bool IsAtext (char c)
		{
			if (c >= 0x80 || IsAsciiLetterOrDigit (c))
				return true;

			switch (c) {
			case '!': case '#': case '$': case '%': case '&': case '\'': case '*': case '+': case '-':
			case '/': case '=': case '?': case '^': case '_': case '`': case '{': case '|': case '}': case '~':
				return true;
			default:
				return false;
			}
		}

		// RFC 5890, Section 2.3.1: an A-label begins with the (case-insensitive) ACE prefix "xn--".
		internal static bool HasAcePrefix (string text, int startIndex, int endIndex)
		{
			return endIndex - startIndex >= 4 &&
				(text[startIndex] == 'x' || text[startIndex] == 'X') &&
				(text[startIndex + 1] == 'n' || text[startIndex + 1] == 'N') &&
				text[startIndex + 2] == '-' && text[startIndex + 3] == '-';
		}

		// Dot-string = Atom *("."  Atom)
		// Atom       = 1*atext
		internal static bool SkipDotString (string text, ref int index, ref bool alabel)
		{
			do {
				int startIndex = index;

				while (index < text.Length && IsAtext (text[index]))
					index++;

				if (index == startIndex)
					return false;

				if (HasAcePrefix (text, startIndex, index))
					alabel = true;

				if (index >= text.Length || text[index] != '.')
					return true;

				index++;
			} while (true);
		}

		// Quoted-string   = DQUOTE *QcontentSMTP DQUOTE
		// QcontentSMTP    = qtextSMTP / quoted-pairSMTP
		// quoted-pairSMTP = %d92 %d32-126
		// qtextSMTP       = %d32-33 / %d35-91 / %d93-126 / UTF8-non-ascii
		internal static bool SkipQuotedString (string text, ref int index)
		{
			// skip over the opening DQUOTE
			index++;

			while (index < text.Length) {
				char c = text[index++];

				if (c == '"')
					return true;

				if (c == '\\') {
					if (index >= text.Length || text[index] < 32 || text[index] > 126)
						return false;

					index++;
				} else if (c < 32 || c == 127) {
					return false;
				}
			}

			return false;
		}

		// A quoted-string (see SkipQuotedString) that may also contain Japanese text encoded using ISO-2022-JP
		// (rfc1468). Some legacy Japanese mail clients produce such local-parts even though the ESC character is
		// not permitted in a quoted-string.
		//
		// The text must end in ASCII, each JIS X 0208 character must be two bytes in the range 0x21-0x7E, and any
		// '"' or '\' byte must be escaped using a quoted-pair, just as it would be anywhere else in a quoted-string.
		static bool SkipIso2022JpQuotedString (string text, ref int index)
		{
			var mode = Iso2022JpMode.Ascii;
			int pending = 0;

			// skip over the opening DQUOTE
			index++;

			while (index < text.Length) {
				char c = text[index++];

				if (c == '"')
					return mode == Iso2022JpMode.Ascii;

				if (c == Escape) {
					// Note: A JIS X 0208 character is two bytes, so a run of them cannot end in the middle of one.
					if (pending != 0 || !SkipIso2022JpEscapeSequence (text, ref index, ref mode))
						return false;

					continue;
				}

				if (c == '\\') {
					if (index >= text.Length || text[index] < 32 || text[index] > 126)
						return false;

					c = text[index++];
				} else if (c < 32 || c == 127) {
					return false;
				}

				if (mode == Iso2022JpMode.JisX0208) {
					if (c < 0x21 || c > 0x7E)
						return false;

					pending ^= 1;
				} else if (mode == Iso2022JpMode.JisRoman && c >= 0x80) {
					return false;
				}
			}

			return false;
		}

		// RFC 1468, Section 2: ISO-2022-JP text starts in ASCII and must end in ASCII. The escape sequences that
		// switch between the character sets are:
		//
		// ESC ( B   ASCII
		// ESC ( J   JIS X 0201-Roman
		// ESC $ @   JIS X 0208-1978
		// ESC $ B   JIS X 0208-1983
		static bool SkipIso2022JpEscapeSequence (string text, ref int index, ref Iso2022JpMode mode)
		{
			if (index + 1 >= text.Length)
				return false;

			char c0 = text[index];
			char c1 = text[index + 1];

			if (c0 == '(' && c1 == 'B')
				mode = Iso2022JpMode.Ascii;
			else if (c0 == '(' && c1 == 'J')
				mode = Iso2022JpMode.JisRoman;
			else if (c0 == '$' && (c1 == '@' || c1 == 'B'))
				mode = Iso2022JpMode.JisX0208;
			else
				return false;

			index += 2;

			return true;
		}

		// Domain     = sub-domain *("." sub-domain)
		// sub-domain = Let-dig [Ldh-str]
		// Let-dig    = ALPHA / DIGIT
		// Ldh-str    = *( ALPHA / DIGIT / "-" ) Let-dig
		//
		// RFC 6531: sub-domain =/ U-label
		internal static bool SkipDomain (string text, ref int index, ref bool alabel)
		{
			do {
				int startIndex = index;

				while (index < text.Length && (text[index] >= 0x80 || text[index] == '-' || IsAsciiLetterOrDigit (text[index])))
					index++;

				if (index == startIndex || text[startIndex] == '-' || text[index - 1] == '-')
					return false;

				if (HasAcePrefix (text, startIndex, index))
					alabel = true;

				if (index >= text.Length || text[index] != '.')
					return true;

				index++;
			} while (true);
		}

		// address-literal  = "[" ( IPv4-address-literal / IPv6-address-literal / General-address-literal ) "]"
		internal static bool SkipAddressLiteral (string text, ref int index)
		{
			int startIndex = index + 1;
			int endIndex = text.IndexOf (']', startIndex);

			if (endIndex == -1)
				return false;

			index = endIndex + 1;

			if (endIndex - startIndex >= 5 && string.Compare (text, startIndex, "IPv6:", 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
				return IsIPv6Address (text, startIndex + 5, endIndex);

			int colon = text.IndexOf (':', startIndex, endIndex - startIndex);

			if (colon != -1)
				return IsGeneralAddressLiteral (text, startIndex, colon, endIndex);

			return IsIPv4Address (text, startIndex, endIndex);
		}

		// IPv4-address-literal = Snum 3("."  Snum)
		// Snum                 = 1*3DIGIT ; representing a decimal integer value in the range 0 through 255
		internal static bool IsIPv4Address (string text, int startIndex, int endIndex)
		{
			int index = startIndex;

			for (int i = 0; i < 4; i++) {
				if (i > 0) {
					if (index >= endIndex || text[index] != '.')
						return false;

					index++;
				}

				int numberIndex = index;
				int value = 0;

				while (index < endIndex && index - numberIndex < 3 && text[index] >= '0' && text[index] <= '9')
					value = (value * 10) + (text[index++] - '0');

				if (index == numberIndex || value > 255)
					return false;
			}

			return index == endIndex;
		}

		// IPv6-addr   = IPv6-full / IPv6-comp / IPv6v4-full / IPv6v4-comp
		// IPv6-hex    = 1*4HEXDIG
		// IPv6-full   = IPv6-hex 7(":" IPv6-hex)
		// IPv6-comp   = [IPv6-hex *5(":" IPv6-hex)] "::" [IPv6-hex *5(":" IPv6-hex)]
		//             ; The "::" represents at least 2 16-bit groups of zeros. No more than 6 groups in addition to the "::" may be present.
		// IPv6v4-full = IPv6-hex 5(":" IPv6-hex) ":" IPv4-address-literal
		// IPv6v4-comp = [IPv6-hex *3(":" IPv6-hex)] "::" [IPv6-hex *3(":" IPv6-hex) ":"] IPv4-address-literal
		//             ; The "::" represents at least 2 16-bit groups of zeros. No more than 4 groups in addition to the "::" and IPv4-address-literal may be present.
		internal static bool IsIPv6Address (string text, int startIndex, int endIndex)
		{
			int compressed = text.IndexOf ("::", startIndex, endIndex - startIndex, StringComparison.Ordinal);
			bool ipv4 = false;
			int groups = 0;

			if (compressed == -1) {
				if (!TryParseIPv6Groups (text, startIndex, endIndex, true, ref groups, ref ipv4))
					return false;

				return groups == (ipv4 ? 6 : 8);
			}

			// only a single "::" is allowed
			if (compressed + 2 < endIndex && text.IndexOf (':', compressed + 2, endIndex - (compressed + 2)) == compressed + 2)
				return false;

			if (compressed > startIndex && !TryParseIPv6Groups (text, startIndex, compressed, false, ref groups, ref ipv4))
				return false;

			if (compressed + 2 < endIndex && !TryParseIPv6Groups (text, compressed + 2, endIndex, true, ref groups, ref ipv4))
				return false;

			return groups <= (ipv4 ? 4 : 6);
		}

		internal static bool TryParseIPv6Groups (string text, int startIndex, int endIndex, bool allowIPv4, ref int groups, ref bool ipv4)
		{
			int index = startIndex;

			do {
				int groupIndex = index;

				while (index < endIndex && index - groupIndex < 4 && IsHexDigit (text[index]))
					index++;

				if (allowIPv4 && index < endIndex && text[index] == '.') {
					// the IPv4-address-literal must be the last component
					ipv4 = true;

					return IsIPv4Address (text, groupIndex, endIndex);
				}

				if (index == groupIndex)
					return false;

				groups++;

				if (index == endIndex)
					return true;

				if (text[index] != ':')
					return false;

				index++;
			} while (index < endIndex);

			// trailing ':'
			return false;
		}

		// General-address-literal = Standardized-tag ":" 1*dcontent
		// Standardized-tag        = Ldh-str
		// dcontent                = %d33-90 / %d94-126
		internal static bool IsGeneralAddressLiteral (string text, int startIndex, int colon, int endIndex)
		{
			if (colon == startIndex || text[colon - 1] == '-')
				return false;

			for (int i = startIndex; i < colon; i++) {
				if (text[i] != '-' && !IsAsciiLetterOrDigit (text[i]))
					return false;
			}

			if (colon + 1 == endIndex)
				return false;

			for (int i = colon + 1; i < endIndex; i++) {
				char c = text[i];

				if (c < 33 || c > 126 || c == '[' || c == '\\')
					return false;
			}

			return true;
		}

	}
}
