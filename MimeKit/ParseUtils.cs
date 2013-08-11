//
// ParseUtils.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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

namespace MimeKit {
	static class ParseUtils
	{
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
				} else {
					if (text[index] == (byte) '(')
						depth++;
					else if (text[index] == (byte) ')')
						depth--;
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
			while (index < endIndex && text[index].IsAtom ())
				index++;

			return true;
		}

		public static bool SkipWord (byte[] text, ref int index, int endIndex, bool throwOnError)
		{
			if (text[index] == (byte) '"')
				return SkipQuoted (text, ref index, endIndex, throwOnError);

			if (text[index].IsAtom ())
				return SkipAtom (text, ref index, endIndex);

			if (throwOnError)
				throw new ParseException (string.Format ("Invalid word token at offset {0}", index), index, index);

			return false;
		}

		static bool TryParseDotAtom (byte[] text, ref int index, int endIndex, bool throwOnError, string tokenType, out string dotatom)
		{
			StringBuilder token = new StringBuilder ();
			int startIndex = index;

			dotatom = null;

			do {
				if (!text[index].IsAtom ()) {
					if (throwOnError)
						throw new ParseException (string.Format ("Invalid {0} token at offset {1}", tokenType, startIndex), startIndex, index);

					return false;
				}

				while (index < endIndex && text[index].IsAtom ()) {
					token.Append ((char) text[index]);
					index++;
				}

				if (!SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex || text[index] != (byte) '.')
					break;

				token.Append ('.');
				index++;

				if (!SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format ("Incomplete {0} token at offset {1}", tokenType, startIndex), startIndex, index);

					return false;
				}
			} while (true);

			dotatom = token.ToString ();

			return true;
		}

		static bool TryParseDomainLiteral (byte[] text, ref int index, int endIndex, bool throwOnError, out string domain)
		{
			StringBuilder token = new StringBuilder ("[");
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

		public static bool TryParseDomain (byte[] text, ref int index, int endIndex, bool throwOnError, out string domain)
		{
			if (text[index] == (byte) '[')
				return TryParseDomainLiteral (text, ref index, endIndex, throwOnError, out domain);

			return TryParseDotAtom (text, ref index, endIndex, throwOnError, "domain", out domain);
		}
	}
}
