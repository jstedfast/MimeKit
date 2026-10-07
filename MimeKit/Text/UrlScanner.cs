//
// UrlScanner.cs
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
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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

		// rfc5321, section 4.5.3.1.1: The maximum total length of a user name or other local-part is 64 octets.
		const int MaxLocalPartLength = 64;

		// rfc5321, section 4.5.3.1.2: The maximum total length of a domain name or number is 255 octets.
		const int MaxDomainLength = 255;

		// rfc1035, section 2.3.4: Labels are limited to 63 octets or less.
		const int MaxLabelLength = 63;

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

		public bool Scan (char[] text, int startIndex, int count, [NotNullWhen (true)] out UrlMatch? match)
		{
			GetIndexDelegate getStartIndex, getEndIndex;
			int endIndex = startIndex + count;
			int searchIndex = startIndex;
			int index;

			// Note: If a pattern is found but it is not part of a valid URL (e.g. a lone '@'), keep searching the remainder of
			// the text. Each failed candidate only examines a bounded amount of text around the pattern, so this remains linear.
			while ((index = trie.Search (text, searchIndex, endIndex - searchIndex, out var pattern)) != -1) {
				searchIndex = index + 1;

				// Note: pattern is not null when Trie.Search != -1
				if (!patterns.TryGetValue (pattern!, out var url))
					continue;

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

				match = new UrlMatch (url.Pattern, url.Prefix);

				if (getStartIndex (match, text, startIndex, index, endIndex) && getEndIndex (match, text, startIndex, index, endIndex))
					return true;
			}

			match = null;

			return false;
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

		// Non-ASCII characters are allowed in order to support IRIs and internationalized addresses, but whitespace
		// (e.g. NBSP or U+2028), control characters, and invisible formatting characters (e.g. bidi overrides or
		// zero-width spaces) are never considered part of a URL since they can be used to disguise the link target.
		static bool IsNonAsciiUrlSafe (char c)
		{
			if (char.IsWhiteSpace (c) || char.IsControl (c))
				return false;

			return char.GetUnicodeCategory (c) != UnicodeCategory.Format;
		}

		static bool IsUrlSafe (char c)
		{
			if (c >= 128)
				return IsNonAsciiUrlSafe (c);

			return IsLetterOrDigit (c) || UrlSafeCharacters.IndexOf (c) != -1;
		}

		static bool IsAtom (char c)
		{
			if (c >= 128)
				return IsNonAsciiUrlSafe (c);

			return IsLetterOrDigit (c) || AtomCharacters.IndexOf (c) != -1;
		}

		static bool IsDomain (char c)
		{
			if (c >= 128)
				return IsNonAsciiUrlSafe (c);

			return IsLetterOrDigit (c) || c == '-';
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

		static bool IsSubDomainStart (char c)
		{
			return IsDomain (c) && c != '-';
		}

		// Note: The caller is responsible for verifying that the first character is a valid sub-domain start character.
		static bool SkipSubDomain (char[] text, int endIndex, ref int index)
		{
			int startIndex = index++;

			// Note: Stop 1 char beyond the maximum label length so that we can detect (and reject) labels that are too long.
			while (index < endIndex && index - startIndex <= MaxLabelLength && IsDomain (text[index]))
				index++;

			return index - startIndex <= MaxLabelLength;
		}

		static bool SkipDomain (char[] text, int endIndex, ref int index)
		{
			return SkipDomain (text, endIndex, index, ref index);
		}

		// Note: domainStartIndex may be less than index if a portion of the domain has already been consumed (e.g. "www.")
		// so that it is counted toward the maximum domain length.
		static bool SkipDomain (char[] text, int endIndex, int domainStartIndex, ref int index)
		{
			// Note: Limit how far forward we scan the domain. We allow scanning 1 char beyond the maximum domain length so
			// that we can detect (and reject) domains that are too long.
			int domainEndIndex = Math.Min (endIndex, domainStartIndex + MaxDomainLength + 1);

			if (index >= domainEndIndex || !IsSubDomainStart (text[index]) || !SkipSubDomain (text, domainEndIndex, ref index))
				return false;

			while (index < domainEndIndex && text[index] == '.') {
				int subdomain = index++;

				if (index == domainEndIndex) {
					// Check if the domain continues beyond the maximum length.
					if (index < endIndex && IsSubDomainStart (text[index]))
						return false;

					index = subdomain;
					break;
				}

				if (!IsSubDomainStart (text[index])) {
					index = subdomain;
					break;
				}

				if (!SkipSubDomain (text, domainEndIndex, ref index))
					return false;
			}

			return index - domainStartIndex <= MaxDomainLength;
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

				// Note: Stop after 4 digits; any more than 3 digits is invalid anyway.
				while (index < endIndex && index - startIndex < 4 && text[index] >= '0' && text[index] <= '9') {
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

				// Note: Stop after 5 hex digits; any more than 4 hex digits is invalid anyway.
				while (index < endIndex && index - startIndex < 5 && IsHexDigit (text[index]))
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

		// A fully-qualified domain has at least 2 labels where the last label (the top-level domain) consists of at least 2 letters.
		static bool IsFullyQualifiedDomain (char[] text, int startIndex, int endIndex)
		{
			int index = endIndex;

			while (index > startIndex && text[index - 1] != '.') {
				if (!char.IsLetter (text[index - 1]))
					return false;

				index--;
			}

			return index > startIndex && endIndex - index >= 2;
		}

		static bool GetAddrspecStartIndex (UrlMatch match, char[] text, int startIndex, int matchIndex, int endIndex)
		{
			int index = matchIndex - 1;

			if (matchIndex == startIndex)
				return false;

			// Note: Limit how far back we scan so that the cost of each candidate is bounded. We allow scanning 1 char beyond
			// the maximum local-part length so that we can detect (and reject) local-parts that are too long.
			int minIndex = Math.Max (startIndex, matchIndex - (MaxLocalPartLength + 1));

			do {
				if (!SkipWordBackwards (text, minIndex, ref index))
					return false;

				if (index == minIndex)
					break;

				if (text[index - 1] != '.')
					break;

				index -= 2;

				if (index < minIndex)
					return false;
			} while (true);

			if (matchIndex - index > MaxLocalPartLength)
				return false;

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
				int domainIndex = index;

				if (!SkipDomain (text, endIndex, ref index))
					return false;

				// Note: To reduce false positives (e.g. "foo@bar"), require bare addresses to have a fully-qualified domain.
				if (!IsFullyQualifiedDomain (text, domainIndex, index))
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

		static bool IsTrailingPunctuation (char c)
		{
			switch (c) {
			case '.': case ',': case ':': case ';': case '!': case '?': case '\'': case '"': case '*':
				return true;
			default:
				return false;
			}
		}

		// Trailing punctuation is far more likely to belong to the surrounding prose than to the URL (e.g. "See
		// http://example.com/path." or "(see http://example.com/path)"), so strip it from the end of the URL. Closing
		// brackets are only stripped if they are unbalanced within the URL so that links such as
		// http://en.wikipedia.org/wiki/Foo_(bar) remain intact.
		static void TrimTrailingPunctuation (char[] text, int startIndex, ref int index)
		{
			int parens = 0, brackets = 0, braces = 0;

			for (int i = startIndex; i < index; i++) {
				switch (text[i]) {
				case '(': parens++; break;
				case ')': parens--; break;
				case '[': brackets++; break;
				case ']': brackets--; break;
				case '{': braces++; break;
				case '}': braces--; break;
				}
			}

			while (index > startIndex) {
				char c = text[index - 1];

				if (IsTrailingPunctuation (c)) {
					index--;
				} else if (c == ')' && parens < 0) {
					parens++;
					index--;
				} else if (c == ']' && brackets < 0) {
					brackets++;
					index--;
				} else if (c == '}' && braces < 0) {
					braces++;
					index--;
				} else {
					break;
				}
			}
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

			TrimTrailingPunctuation (text, matchIndex + match.Pattern.Length, ref index);

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
			// Note: Limit how far forward we scan the local-part so that the cost of each candidate is bounded. We allow
			// scanning 1 char beyond the maximum local-part length so that we can detect (and reject) local-parts that
			// are too long.
			int localPartEndIndex = Math.Min (endIndex, index + MaxLocalPartLength + 1);
			int localPartStartIndex = index;

			if (!SkipWord (text, localPartEndIndex, ref index) || index >= localPartEndIndex)
				return false;

			while (text[index] == '.') {
				index++;

				if (index >= localPartEndIndex)
					return false;

				if (!SkipWord (text, localPartEndIndex, ref index))
					return false;

				if (index >= localPartEndIndex)
					return false;
			}

			if (index - localPartStartIndex > MaxLocalPartLength)
				return false;

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
				int queryIndex = index;

				index++;

				while (index < endIndex && IsUrlSafe (text[index]) && text[index] != close)
					index++;

				TrimTrailingPunctuation (text, queryIndex, ref index);
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

			// Note: For patterns such as "www." and "ftp.", the pattern is part of the hostname.
			int hostIndex = match.Pattern[match.Pattern.Length - 1] == '.' ? matchIndex : index;

			if (index >= endIndex || !SkipDomain (text, endIndex, hostIndex, ref index))
				return false;

			// check for a port
			if (index + 1 < endIndex && text[index] == ':' && IsDigit (text[index + 1])) {
				index += 2;

				while (index < endIndex && IsDigit (text[index]))
					index++;
			}

			// check for a path or query in cases where the link looks like this: https://www.domain.com?query
			if (index < endIndex && (text[index] == '/' || text[index] == '?')) {
				int pathIndex = index;

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

				TrimTrailingPunctuation (text, pathIndex, ref index);
			}

			match.EndIndex = index;

			return true;
		}
	}
}
