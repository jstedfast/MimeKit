//
// ParseUtils.cs
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
using System.Text;
using System.Globalization;

namespace MimeKit.Utils {
	static class ParseUtils
	{
		public static void ValidateArguments (ParserOptions options, byte[] buffer, int startIndex, int length)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (length));
		}

		public static void ValidateArguments (ParserOptions options, byte[] buffer, int startIndex)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));
		}

		public static void ValidateArguments (ParserOptions options, byte[] buffer)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));
		}

		public static void ValidateArguments (ParserOptions options, string text)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (text is null)
				throw new ArgumentNullException (nameof (text));
		}

		public static void ValidateArguments (byte[] buffer, int startIndex, int length)
		{
			if (buffer is null)
				throw new ArgumentNullException (nameof (buffer));

			if (startIndex < 0 || startIndex > buffer.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (length < 0 || length > (buffer.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (length));
		}

		public static bool TryParseInt32 (byte[] text, ref int index, int endIndex, out int value)
		{
			int startIndex = index;

			value = 0;

			while (index < endIndex && text[index] >= (byte) '0' && text[index] <= (byte) '9') {
				int digit = text[index] - (byte) '0';

				if (value > int.MaxValue / 10) {
					// integer overflow
					return false;
				}

				if (value == int.MaxValue / 10 && digit > int.MaxValue % 10) {
					// integer overflow
					return false;
				}

				value = (value * 10) + digit;
				index++;
			}

			return index > startIndex;
		}

		public static bool SkipWhiteSpace (byte[] text, ref int index, int endIndex)
		{
			int startIndex = index;

			while (index < endIndex && text[index].IsWhitespace ())
				index++;

			return index > startIndex;
		}

		public static bool SkipComment (byte[] text, ref int index, int endIndex)
		{
			bool escaped = false;
			int depth = 1;

			index++;

			while (index < endIndex && depth > 0) {
				if (text[index] == (byte) '\\') {
					escaped = !escaped;
				} else if (!escaped) {
					if (text[index] == (byte) '(')
						depth++;
					else if (text[index] == (byte) ')')
						depth--;
					escaped = false;
				} else {
					escaped = false;
				}

				index++;
			}

			return depth == 0;
		}

		public static bool SkipComment (string text, ref int index, int endIndex)
		{
			bool escaped = false;
			int depth = 1;

			index++;

			while (index < endIndex && depth > 0) {
				if (text[index] == '\\') {
					escaped = !escaped;
				} else if (!escaped) {
					if (text[index] == '(')
						depth++;
					else if (text[index] == ')')
						depth--;
					escaped = false;
				} else {
					escaped = false;
				}

				index++;
			}

			return depth == 0;
		}

		public static bool SkipCommentsAndWhiteSpace (byte[] text, ref int index, int endIndex, bool throwOnError)
		{
			SkipWhiteSpace (text, ref index, endIndex);

			while (index < endIndex && text[index] == (byte) '(') {
				int startIndex = index;

				if (!SkipComment (text, ref index, endIndex)) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete comment token at offset {0}", startIndex), startIndex, index);

					return false;
				}

				SkipWhiteSpace (text, ref index, endIndex);
			}

			return true;
		}

		public static bool SkipQuoted (byte[] text, ref int index, int endIndex, bool throwOnError)
		{
			int startIndex = index;
			bool escaped = false;

			// skip over leading '"'
			index++;

			while (index < endIndex) {
				if (text[index] == (byte) '\\') {
					escaped = !escaped;
				} else if (!escaped) {
					if (text[index] == (byte) '"')
						break;
				} else {
					escaped = false;
				}

				index++;
			}

			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete quoted-string token at offset {0}", startIndex), startIndex, index);

				return false;
			}

			// skip over the closing '"'
			index++;

			return true;
		}

		public static bool SkipAtom (byte[] text, ref int index, int endIndex)
		{
			int start = index;

			while (index < endIndex && text[index].IsAtom ())
				index++;

			return index > start;
		}

		// Note: a "phrase atom" is a more lenient atom (e.g. mailbox display-name phrase atom)
		public static bool SkipPhraseAtom (byte[] text, ref int index, int endIndex)
		{
			int start = index;

			while (index < endIndex && text[index].IsPhraseAtom ())
				index++;

			return index > start;
		}

		public static bool SkipToken (byte[] text, ref int index, int endIndex)
		{
			int start = index;

			while (index < endIndex && text[index].IsToken ())
				index++;

			return index > start;
		}

		public static bool SkipWord (byte[] text, ref int index, int endIndex, bool throwOnError)
		{
			if (text[index] == (byte) '"')
				return SkipQuoted (text, ref index, endIndex, throwOnError);

			if (text[index].IsAtom ())
				return SkipAtom (text, ref index, endIndex);

			return false;
		}

		public static bool IsSentinel (byte c, ReadOnlySpan<byte> sentinels)
		{
			for (int i = 0; i < sentinels.Length; i++) {
				if (c == sentinels[i])
					return true;
			}

			return false;
		}

		static bool TryParseDotAtom (byte[] text, ref int index, int endIndex, ReadOnlySpan<byte> sentinels, bool throwOnError, string tokenType, out string dotatom)
		{
			using var token = new ValueStringBuilder (128);
			int startIndex = index;
			int comment;

			dotatom = null;

			do {
				if (!text[index].IsAtom ()) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid {0} token at offset {1}", tokenType, startIndex), startIndex, index);

					return false;
				}

				int start = index;
				while (index < endIndex && text[index].IsAtom ())
					index++;

				try {
					token.Append (CharsetUtils.UTF8.GetString (text, start, index - start));
				} catch (DecoderFallbackException ex) {
					if (throwOnError)
						throw new ParseException ("Internationalized domains may only contain UTF-8 characters.", start, start, ex);

					return false;
				}

				comment = index;
				if (!SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex || text[index] != (byte) '.') {
					index = comment;
					break;
				}

				// skip over the '.'
				index++;

				if (!SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				// allow domains to end with a '.', but strip it off
				if (index >= endIndex || IsSentinel (text[index], sentinels))
					break;

				token.Append ('.');
			} while (true);

			dotatom = token.ToString ();

			return true;
		}

		static bool TryParseDomainLiteral (byte[] text, ref int index, int endIndex, bool throwOnError, out string domain)
		{
			using var token = new ValueStringBuilder (128);
			token.Append('[');
			int startIndex = index++;

			domain = null;

			SkipWhiteSpace (text, ref index, endIndex);

			do {
				while (index < endIndex && text[index].IsDomain ()) {
					token.Append ((char) text[index]);
					index++;
				}

				SkipWhiteSpace (text, ref index, endIndex);

				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete domain literal token at offset {0}", startIndex), startIndex, index);

					return false;
				}

				if (text[index] == (byte) ']')
					break;

				if (!text[index].IsDomain ()) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid domain literal token at offset {0}", startIndex), startIndex, index);

					return false;
				}
			} while (true);

			token.Append (']');
			index++;

			domain = token.ToString ();

			return true;
		}

		public static bool TryParseDomain (byte[] text, ref int index, int endIndex, ReadOnlySpan<byte> sentinels, bool throwOnError, out string domain)
		{
			if (text[index] == (byte) '[')
				return TryParseDomainLiteral (text, ref index, endIndex, throwOnError, out domain);

			return TryParseDotAtom (text, ref index, endIndex, sentinels, throwOnError, "domain", out domain);
		}

		static ReadOnlySpan<byte> GreaterThanOrAt => ">@"u8;

		public static bool TryParseMsgId (byte[] text, ref int index, int endIndex, bool requireAngleAddr, bool throwOnError, out string msgid)
		{
			// const CharType SpaceOrControl = CharType.IsWhitespace | CharType.IsControl;
			var squareBrackets = false;
			var angleAddr = false;

			msgid = null;

			if (!SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex || (requireAngleAddr && text[index] != '<')) {
				if (throwOnError)
					throw new ParseException ("No msg-id token found.", index, index);

				return false;
			}

			int tokenIndex = index;

			if (text[index] == '<') {
				angleAddr = true;
				index++;
			}

			SkipWhiteSpace (text, ref index, endIndex);

			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete msg-id token at offset {0}", tokenIndex), tokenIndex, index);

				return false;
			}

			using var token = new ValueStringBuilder (128);

			if (text[index] == '[') {
				// Note: This seems to be a bug in Microsoft Exchange??
				// See https://github.com/jstedfast/MimeKit/issues/912
				squareBrackets = true;
			}

			// consume the local-part of the msg-id using a very loose definition of 'local-part'
			//
			// See https://github.com/jstedfast/MimeKit/issues/472 for the reasons why.
			do {
				int start = index;

				if (text[index] == '"') {
					if (!SkipQuoted (text, ref index, endIndex, throwOnError))
						return false;
				} else {
					while (index < endIndex && text[index] != (byte) '.' && text[index] != (byte) '@' && text[index] != (byte) '>' && !text[index].IsWhitespace ())
						index++;
				}

				try {
					token.Append (CharsetUtils.UTF8.GetString (text, start, index - start));
				} catch (DecoderFallbackException ex) {
					if (throwOnError)
						throw new ParseException ("Internationalized local-part tokens may only contain UTF-8 characters.", start, start, ex);

					return false;
				}

				SkipWhiteSpace (text, ref index, endIndex);

				if (index >= endIndex) {
					if (angleAddr) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete msg-id at offset {0}", tokenIndex), tokenIndex, index);

						return false;
					}

					// since the msg-id token did not start with a '<', we do not need a '>'
					break;
				}

				if (text[index] == (byte) '@' || text[index] == (byte) '>')
					break;

				if (text[index] == (byte) '.') {
					token.Append ('.');
					index++;

					SkipWhiteSpace (text, ref index, endIndex);
				}

				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete msg-id at offset {0}", tokenIndex), tokenIndex, index);

					return false;
				}
			} while (true);

			if (index < endIndex && text[index] == (byte) '@') {
				token.Append ('@');
				index++;

				// Note: some Message-Id's are broken and in the form "<local-part@@domain>"
				//
				// See https://github.com/jstedfast/MimeKit/issues/962 for details.
				while (index < endIndex && text[index] == (byte) '@')
					index++;

				if (!SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index < endIndex && text[index] != (byte) '>') {
					// Note: some Message-Id's are broken and in the form "<local-part@domain1@domain2>"
					// https://github.com/jstedfast/MailKit/issues/138
					do {
						if (!TryParseDomain (text, ref index, endIndex, GreaterThanOrAt, throwOnError, out string domain))
							return false;

						if (IsIdnEncoded (domain))
							domain = MailboxAddress.IdnMapping.Decode (domain);

						token.Append (domain);

						if (index >= endIndex || text[index] != (byte) '@')
							break;

						token.Append ('@');
						index++;
					} while (true);

					if (!SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;
				} else {
					// The msgid token was in the form "<local-part@>". Technically this is illegal, but for
					// the sake of maximum compatibility, I guess we have no choice but to accept it...
					// https://github.com/jstedfast/MimeKit/issues/102

					//if (throwOnError)
					//	throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete msg-id token at offset {0}", tokenIndex), tokenIndex, index);

					//return false;
				}
			}

			if (squareBrackets && index < endIndex && text[index] == (byte) ']') {
				token.Append (']');
				index++;
			}

			if (angleAddr && (index >= endIndex || text[index] != '>')) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete msg-id token at offset {0}", tokenIndex), tokenIndex, index);

				return false;
			}

			if (index < endIndex && text[index] == (byte) '>')
				index++;

			msgid = token.ToString ();

			return true;
		}

		public static bool IsInternational (string value, int startIndex, int count)
		{
			int endIndex = startIndex + count;

			for (int i = startIndex; i < endIndex; i++) {
				if (value[i] > 127)
					return true;
			}

			return false;
		}

		public static bool IsInternational (string value, int startIndex)
		{
			return IsInternational (value, startIndex, value.Length - startIndex);
		}

		public static bool IsInternational (string value)
		{
			return IsInternational (value, 0, value.Length);
		}

		public static bool IsIdnEncoded (string value)
		{
			if (value.StartsWith ("xn--", StringComparison.Ordinal))
				return true;

			return value.IndexOf (".xn--", StringComparison.Ordinal) != -1;
		}
    }
}
