//
// ParseUtils.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

using System.Text;

#if PORTABLE
using EncoderReplacementFallback = Portable.Text.EncoderReplacementFallback;
using DecoderReplacementFallback = Portable.Text.DecoderReplacementFallback;
using EncoderExceptionFallback = Portable.Text.EncoderExceptionFallback;
using DecoderExceptionFallback = Portable.Text.DecoderExceptionFallback;
using EncoderFallbackException = Portable.Text.EncoderFallbackException;
using DecoderFallbackException = Portable.Text.DecoderFallbackException;
using DecoderFallbackBuffer = Portable.Text.DecoderFallbackBuffer;
using DecoderFallback = Portable.Text.DecoderFallback;
using Encoding = Portable.Text.Encoding;
using Encoder = Portable.Text.Encoder;
using Decoder = Portable.Text.Decoder;
#endif

namespace MimeKit.Utils {
	static class ParseUtils
	{
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

		public static bool SkipCommentsAndWhiteSpace (byte[] text, ref int index, int endIndex, bool throwOnError)
		{
			SkipWhiteSpace (text, ref index, endIndex);

			while (index < endIndex && text[index] == (byte) '(') {
				int startIndex = index;

				if (!SkipComment (text, ref index, endIndex)) {
					if (throwOnError)
						throw new ParseException (string.Format ("Incomplete comment token at offset {0}", startIndex), startIndex, index);

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

			if (index >= endIndex || text[index] != (byte) '"') {
				if (throwOnError)
					throw new ParseException (string.Format ("Incomplete quoted-string token at offset {0}", startIndex), startIndex, index);

				return false;
			}

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

		static bool IsSentinel (byte c, byte[] sentinels)
		{
			for (int i = 0; i < sentinels.Length; i++) {
				if (c == sentinels[i])
					return true;
			}

			return false;
		}

		static bool TryParseDotAtom (byte[] text, ref int index, int endIndex, byte[] sentinels, bool throwOnError, string tokenType, out string dotatom)
		{
			var token = new StringBuilder ();
			int startIndex = index;
			int comment;

			dotatom = null;

			do {
				if (!text[index].IsAtom ()) {
					if (throwOnError)
						throw new ParseException (string.Format ("Invalid {0} token at offset {1}", tokenType, startIndex), startIndex, index);

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
			var token = new StringBuilder ("[");
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
						throw new ParseException (string.Format ("Incomplete domain literal token at offset {0}", startIndex), startIndex, index);

					return false;
				}

				if (text[index] == (byte) ']')
					break;

				if (!text[index].IsDomain ()) {
					if (throwOnError)
						throw new ParseException (string.Format ("Invalid domain literal token at offset {0}", startIndex), startIndex, index);

					return false;
				}
			} while (true);

			token.Append (']');
			index++;

			domain = token.ToString ();

			return true;
		}

		public static bool TryParseDomain (byte[] text, ref int index, int endIndex, byte[] sentinels, bool throwOnError, out string domain)
		{
			if (text[index] == (byte) '[')
				return TryParseDomainLiteral (text, ref index, endIndex, throwOnError, out domain);

			return TryParseDotAtom (text, ref index, endIndex, sentinels, throwOnError, "domain", out domain);
		}
	}
}
