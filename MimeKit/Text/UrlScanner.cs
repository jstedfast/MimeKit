//
// UrlScanner.cs
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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit.Text {
	class UrlMatch
	{
		public readonly string Pattern;
		public readonly string Prefix;
		public int StartIndex;
		public int EndIndex;

		public UrlMatch (string pattern, string prefix)
		{
			Pattern = pattern;
			Prefix = prefix;
		}
	}

	enum UrlPatternType {
		Addrspec,
		MailTo,
		File,
		Web
	}

	class UrlPattern
	{
		public readonly UrlPatternType Type;
		public readonly string Pattern;
		public readonly string Prefix;

		public UrlPattern (UrlPatternType type, string pattern, string prefix)
		{
			Pattern = pattern;
			Prefix = prefix;
			Type = type;
		}
	}

	class UrlScanner
	{
		delegate bool GetIndexDelegate (UrlMatch match, char[] input, int startIndex, int matchIndex, int endIndex);
		const string AtomCharacters = "!#$%&'*+-/=?^_`{|}~";
		const string UrlSafeCharacters = "$-_.+!*'(),{}|\\^~[]`#%\";/?:@&=";

		readonly Dictionary<string, UrlPattern> patterns = new Dictionary<string, UrlPattern> (StringComparer.Ordinal);
		readonly Trie trie = new Trie (true);

		public UrlScanner ()
		{
		}

		public void Add (UrlPattern pattern)
		{
			patterns.Add (pattern.Pattern, pattern);
			trie.Add (pattern.Pattern);
		}

		public bool Scan (char[] text, int startIndex, int count, out UrlMatch match)
		{
			GetIndexDelegate getStartIndex, getEndIndex;
			int endIndex = startIndex + count;
			int index;

			if ((index = trie.Search (text, startIndex, count, out var pattern)) == -1) {
				match = null;
				return false;
			}

			if (!patterns.TryGetValue (pattern, out var url)) {
				match = null;
				return false;
			}

			match = new UrlMatch (url.Pattern, url.Prefix);

			switch (url.Type) {
			case UrlPatternType.Addrspec:
				getStartIndex = GetAddrspecStartIndex;
				getEndIndex = GetAddrspecEndIndex;
				break;
			case UrlPatternType.MailTo:
				getStartIndex = GetMailToStartIndex;
				getEndIndex = GetMailToEndIndex;
				break;
			case UrlPatternType.File:
				getStartIndex = GetFileStartIndex;
				getEndIndex = GetFileEndIndex;
				break;
			default:
				getStartIndex = GetWebStartIndex;
				getEndIndex = GetWebEndIndex;
				break;
			}

			if (!getStartIndex (match, text, startIndex, index, endIndex)) {
				match = null;
				return false;
			}

			if (!getEndIndex (match, text, startIndex, index, endIndex)) {
				match = null;
				return false;
			}

			return true;
		}

		static char GetClosingBrace (UrlMatch match, char[] text, int startIndex)
		{
			if (match.StartIndex == startIndex)
				return '\0';

			switch (text[match.StartIndex - 1]) {
			case '(': return ')';
			case '{': return '}';
			case '[': return ']';
			case '<': return '>';
			case '|': return '|';
			default: return '\0';
			}
		}

		static bool IsDigit (char c)
		{
			return c >= '0' && c <= '9';
		}

		static bool IsLetterOrDigit (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || IsDigit (c);
		}

		static bool IsUrlSafe (char c)
		{
			return c >= 128 || IsLetterOrDigit (c) || UrlSafeCharacters.IndexOf (c) != -1;
		}

		static bool IsAtom (char c)
		{
			return c >= 128 || IsLetterOrDigit (c) || AtomCharacters.IndexOf (c) != -1;
		}

		static bool IsDomain (char c)
		{
			return c >= 128 || IsLetterOrDigit (c) || c == '-';
		}

		static bool SkipAtom (char[] text, int endIndex, ref int index)
		{
			int startIndex = index;

			while (index < endIndex && IsAtom (text[index]))
				index++;

			return index > startIndex;
		}

		static bool SkipAtomBackwards (char[] text, int startIndex, ref int index)
		{
			if (!IsAtom (text[index]))
				return false;

			while (index > startIndex && IsAtom (text[index - 1]))
				index--;

			return true;
		}

		static bool SkipSubDomain (char[] text, int endIndex, ref int index)
		{
			if (!IsDomain (text[index]) || text[index] == '-')
				return false;

			index++;

			while (index < endIndex && IsDomain (text[index]))
				index++;

			return true;
		}

		static bool SkipDomain (char[] text, int endIndex, ref int index)
		{
			if (!SkipSubDomain (text, endIndex, ref index))
				return false;

			while (index < endIndex && text[index] == '.') {
				int subdomain = index++;

				if (index == endIndex || !SkipSubDomain (text, endIndex, ref index)) {
					index = subdomain;
					break;
				}
			}

			return true;
		}

		static bool SkipQuoted (char[] text, int endIndex, ref int index)
		{
			bool escaped = false;

			// skip over leading '"'
			index++;

			while (index < endIndex) {
				if (text[index] == '\\') {
					escaped = !escaped;
				} else if (!escaped) {
					if (text[index] == '"')
						break;
				} else {
					escaped = false;
				}

				index++;
			}

			if (index >= endIndex || text[index] != '"')
				return false;

			index++;

			return true;
		}

		static bool SkipQuotedBackwards (char[] text, int startIndex, ref int index)
		{
			// skip over end quote
			index--;

			while (index >= startIndex) {
				if (text[index] == '"') {
					if (index == startIndex || text[index - 1] != '\\')
						break;
				}

				index--;
			}

			if (index < startIndex || text[index] != '"')
				return false;

			return true;
		}

		static bool SkipWord (char[] text, int endIndex, ref int index)
		{
			if (text[index] == '"')
				return SkipQuoted (text, endIndex, ref index);

			return SkipAtom (text, endIndex, ref index);
		}

		static bool SkipWordBackwards (char[] text, int startIndex, ref int index)
		{
			if (text[index] == '"')
				return SkipQuotedBackwards (text, startIndex, ref index);

			return SkipAtomBackwards (text, startIndex, ref index);
		}

		static bool SkipIPv4Literal (char[] text, int endIndex, ref int index)
		{
			int groups = 0;

			while (index < endIndex && groups < 4) {
				int startIndex = index;
				int value = 0;

				while (index < endIndex && text[index] >= '0' && text[index] <= '9') {
					value = (value * 10) + (text[index] - '0');
					index++;
				}

				if (index == startIndex || index - startIndex > 3 || value > 255)
					return false;

				groups++;

				if (groups < 4 && index < endIndex && text[index] == '.')
					index++;
			}

			return groups == 4;
		}

		static bool IsHexDigit (char c)
		{
			return (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f') || (c >= '0' && c <= '9');
		}

		static bool IsIPv6 (char[] text, int startIndex)
		{
			int index = startIndex;

			if (text[index] != 'I' && text[index] != 'i')
				return false;

			index++;

			if (text[index] != 'P' && text[index] != 'p')
				return false;

			index++;

			if (text[index] != 'V' && text[index] != 'v')
				return false;

			index++;

			return text[index] == '6' && text[index + 1] == ':';
		}

		// This needs to handle the following forms:
		//
		// IPv6-addr = IPv6-full / IPv6-comp / IPv6v4-full / IPv6v4-comp
		// IPv6-hex  = 1*4HEXDIG
		// IPv6-full = IPv6-hex 7(":" IPv6-hex)
		// IPv6-comp = [IPv6-hex *5(":" IPv6-hex)] "::" [IPv6-hex *5(":" IPv6-hex)]
		//             ; The "::" represents at least 2 16-bit groups of zeros
		//             ; No more than 6 groups in addition to the "::" may be
		//             ; present
		// IPv6v4-full = IPv6-hex 5(":" IPv6-hex) ":" IPv4-address-literal
		// IPv6v4-comp = [IPv6-hex *3(":" IPv6-hex)] "::"
		//               [IPv6-hex *3(":" IPv6-hex) ":"] IPv4-address-literal
		//             ; The "::" represents at least 2 16-bit groups of zeros
		//             ; No more than 4 groups in addition to the "::" and
		//             ; IPv4-address-literal may be present
		static bool SkipIPv6Literal (char[] text, int endIndex, ref int index)
		{
			bool compact = false;
			int colons = 0;

			while (index < endIndex) {
				int startIndex = index;

				while (index < endIndex && IsHexDigit (text[index]))
					index++;

				if (index >= endIndex)
					break;

				if (index > startIndex && colons > 2 && text[index] == '.') {
					// IPv6v4
					index = startIndex;

					if (!SkipIPv4Literal (text, endIndex, ref index))
						return false;

					return compact ? colons < 6 : colons == 6;
				}

				int count = index - startIndex;
				if (count > 4)
					return false;

				if (text[index] != ':')
					break;

				startIndex = index;
				while (index < endIndex && text[index] == ':')
					index++;

				count = index - startIndex;
				if (count > 2)
					return false;

				if (count == 2) {
					if (compact)
						return false;

					compact = true;
					colons += 2;
				} else {
					colons++;
				}
			}

			if (colons < 2)
				return false;

			return compact ? colons < 7 : colons == 7;
		}

		static bool GetAddrspecStartIndex (UrlMatch match, char[] text, int startIndex, int matchIndex, int endIndex)
		{
			int index = matchIndex - 1;

			if (matchIndex == startIndex)
				return false;

			do {
				if (!SkipWordBackwards (text, startIndex, ref index))
					return false;

				if (index == startIndex)
					break;

				if (text[index - 1] != '.')
					break;

				index -= 2;

				if (index <= startIndex)
					return false;
			} while (true);

			match.StartIndex = index;

			return true;
		}

		static bool GetAddrspecEndIndex (UrlMatch match, char[] text, int startIndex, int matchIndex, int endIndex)
		{
			int index = matchIndex + 1;

			if (index == endIndex)
				return false;

			if (text[index] != '[') {
				// domain
				if (!SkipDomain (text, endIndex, ref index))
					return false;

				match.EndIndex = index;

				return true;
			}

			// address literal
			index++;

			// we need at least 8 more characters
			if (index + 8 >= endIndex)
				return false;
			
			if (IsIPv6 (text, index)) {
				index += "IPv6:".Length;
				if (!SkipIPv6Literal (text, endIndex, ref index))
					return false;
			} else {
				if (!SkipIPv4Literal (text, endIndex, ref index))
					return false;
			}

			if (index >= endIndex || text[index++] != ']')
				return false;

			match.EndIndex = index;

			return true;
		}

		static bool GetFileStartIndex (UrlMatch match, char[] text, int startIndex, int matchIndex, int endIndex)
		{
			match.StartIndex = matchIndex;
			return true;
		}

		static bool GetFileEndIndex (UrlMatch match, char[] text, int startIndex, int matchIndex, int endIndex)
		{
			char close = GetClosingBrace (match, text, startIndex);
			int index = matchIndex + match.Pattern.Length;

			while (index < endIndex && IsUrlSafe (text[index]) && text[index] != close)
				index++;

			match.EndIndex = index;

			return index > matchIndex + match.Pattern.Length;
		}

		static bool GetMailToStartIndex (UrlMatch match, char[] text, int startIndex, int matchIndex, int endIndex)
		{
			match.StartIndex = matchIndex;
			return true;
		}

		static bool SkipAddrspec (char[] text, int endIndex, ref int index)
		{
			if (!SkipWord (text, endIndex, ref index) || index >= endIndex)
				return false;

			while (text[index] == '.') {
				index++;

				if (index >= endIndex)
					return false;

				if (!SkipWord (text, endIndex, ref index))
					return false;

				if (index >= endIndex)
					return false;
			}

			if (index + 1 >= endIndex || text[index++] != '@')
				return false;

			if (text[index] != '[') {
				// domain
				if (!SkipDomain (text, endIndex, ref index))
					return false;
			} else {
				// address literal
				index++;

				// we need at least 8 more characters
				if (index + 8 >= endIndex)
					return false;

				if (IsIPv6 (text, index)) {
					index += "IPv6:".Length;
					if (!SkipIPv6Literal (text, endIndex, ref index))
						return false;
				} else {
					if (!SkipIPv4Literal (text, endIndex, ref index))
						return false;
				}

				if (index >= endIndex || text[index++] != ']')
					return false;
			}

			return true;
		}

		static bool GetMailToEndIndex (UrlMatch match, char[] text, int startIndex, int matchIndex, int endIndex)
		{
			char close = GetClosingBrace (match, text, startIndex);
			int contentIndex = matchIndex + match.Pattern.Length;
			int index = contentIndex;

			if (contentIndex >= endIndex)
				return false;

			if (!SkipAddrspec (text, endIndex, ref index))
				index = contentIndex;

			if (index < endIndex && text[index] == '?') {
				index++;

				while (index < endIndex && IsUrlSafe (text[index]) && text[index] != close)
					index++;
			}

			match.EndIndex = index;

			return index > contentIndex;
		}

		static bool GetWebStartIndex (UrlMatch match, char[] text, int startIndex, int matchIndex, int endIndex)
		{
			match.StartIndex = matchIndex;
			return true;
		}

		static bool GetWebEndIndex (UrlMatch match, char[] text, int startIndex, int matchIndex, int endIndex)
		{
			char close = GetClosingBrace (match, text, startIndex);
			int index = matchIndex + match.Pattern.Length;

			if (index >= endIndex || !SkipDomain (text, endIndex, ref index))
				return false;

			// check for a port
			if (index + 1 < endIndex && text[index] == ':' && IsDigit (text[index + 1])) {
				index += 2;

				while (index < endIndex && IsDigit (text[index]))
					index++;
			}

			// check for a path or query in cases where the link looks like this: https://www.domain.com?query
			if (index < endIndex && (text[index] == '/' || text[index] == '?')) {
				if (text[index] == '/')
					index++;

				while (index < endIndex && text[index] != close) {
					if (text[index] == '?' || text[index] == '&') {
						if (index + 1 >= endIndex || !char.IsLetterOrDigit (text[index + 1]))
							break;

						index++;
					} else if (!IsUrlSafe (text[index])) {
						break;
					}

					index++;
				}
			}

			match.EndIndex = index;

			return true;
		}
	}
}
