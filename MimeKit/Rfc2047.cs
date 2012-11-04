//
// Rfc2047.cs
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
using System.Collections;
using System.Collections.Generic;

namespace MimeKit {
	public static class Rfc2047
	{
		public static bool EnableWorkarounds {
			get; set;
		}

		class Token {
			public ContentEncoding Encoding;
			public Encoding Charset;
			public int StartIndex;
			public int Length;
			public bool Is8bit;

			public Token (int startIndex, int length)
			{
				Encoding = ContentEncoding.Default;
				StartIndex = startIndex;
				Length = length;
			}
		}

		static bool IsAscii (byte c)
		{
			return c.IsAscii ();
		}

		static bool IsAtom (byte c)
		{
			return c.IsAtom ();
		}

		static bool IsBbQq (byte c)
		{
			return c == 'B' || c == 'b' || c == 'Q' || c == 'q';
		}

		static bool IsLwsp (byte c)
		{
			return c.IsWhitespace ();
		}

		static unsafe bool TryGetEncodedWordToken (byte* input, byte* word, int length, out Token token)
		{
			token = null;

			if (length < 7)
				return false;

			byte* inend = word + length - 2;
			byte* inptr = word;

			// check if this could even be an encoded-word token
			if (*inptr++ != '=' || *inptr++ != '?' || *inend++ != '?' || *inend++ != '=')
				return false;

			inend -= 2;

			if (*inptr == '?' || *inptr == '*') {
				// this would result in an empty charset
				return false;
			}

			var charset = new StringBuilder ();

			// find the end of the charset name
			while (inptr < inend && *inptr != '?' && *inptr != '*') {
				if (!IsAtom (*inptr))
					return false;

				charset.Append ((char) *inptr);
				inptr++;
			}

			if (*inptr == '*') {
				// we found a language code...
				inptr++;

				// find the end of the language code
				while (inptr < inend && *inptr != '?') {
					if (!IsAtom (*inptr))
						return false;

					inptr++;
				}
			}

			if (inptr == inend)
				return false;

			// skip over the '?' to get to the encoding
			inptr++;

			ContentEncoding encoding;
			if (*inptr == 'B' || *inptr == 'b') {
				encoding = ContentEncoding.Base64;
			} else if (*inptr == 'Q' || *inptr == 'q') {
				encoding = ContentEncoding.QuotedPrintable;
			} else {
				return false;
			}

			// skip over the encoding
			inptr++;

			if (*inptr != '?' || inptr == inend)
				return false;

			// skip over the '?' to get to the payload
			inptr++;

			token = new Token ((int) (inptr - input), (int) (inend - inptr));
			token.Charset = CharsetUtils.GetEncoding (charset.ToString ());
			token.Encoding = encoding;

			return true;
		}

		static unsafe List<Token> TokenizePhrase (byte* input, int length)
		{
			List<Token> tokens = new List<Token> ();
			byte* text, word, inptr = input;
			byte* inend = input + length;
			bool encoded = false;
			Token token = null;
			Token lwsp = null;
			bool ascii;
			int n;

			while (inptr < inend) {
				text = inptr;
				while (inptr < inend && IsLwsp (*inptr))
					inptr++;

				if (inptr > text)
					lwsp = new Token ((int) (inptr - input), (int) (inptr - text));
				else
					lwsp = null;

				word = inptr;
				ascii = true;
				if (inptr < inend && IsAtom (*inptr)) {
					if (EnableWorkarounds) {
						// Make an extra effort to detect and separate encoded-word
						// tokens that have been merged with other words.
						bool is_rfc2047 = false;

						if (inptr + 2 < inend && *inptr == '=' && *(inptr + 1) == '?') {
							inptr += 2;

							// skip past the charset (if one is even declared, sigh)
							while (inptr < inend && *inptr != '?') {
								ascii = ascii && IsAscii (*inptr);
								inptr++;
							}

							// sanity check encoding type
							if (inptr + 3 >= inend || *inptr != '?' || !IsBbQq (*(inptr + 1)) || *(inptr + 2) != '?') {
								ascii = true;
								goto non_rfc2047;
							}

							inptr += 3;

							// find the end of the rfc2047 encoded word token
							while (inptr + 2 < inend && *inptr != '?' && *(inptr + 1) != '=') {
								ascii = ascii && IsAscii (*inptr);
								inptr++;
							}

							if (inptr + 2 >= inend) {
								// didn't find an end marker...
								inptr = word + 2;
								ascii = true;

								goto non_rfc2047;
							}

							is_rfc2047 = true;
							inptr += 2;
						}

					non_rfc2047:
						if (!is_rfc2047) {
							// stop if we encounter a possible rfc2047 encoded
							// token even if it's inside another word, sigh.
							while (inptr < inend && IsAtom (*inptr)) {
								if (inptr + 2 < inend && *inptr == '=' && *(inptr + 1) == '?')
									break;
								inptr++;
							}
						}
					} else {
						// encoded-word tokens are atoms
						while (inptr < inend && IsAtom (*inptr))
							inptr++;
					}

					n = (int) (inptr - word);
					if (TryGetEncodedWordToken (input, word, n, out token)) {
						// rfc2047 states that you must ignore all whitespace between
						// encoded-word tokens
						if (!encoded && lwsp != null) {
							// previous token was not encoded, so preserve whitespace
							tokens.Add (lwsp);
						}

						tokens.Add (token);
						encoded = true;
					} else {
						// append the lwsp and atom tokens
						if (lwsp != null)
							tokens.Add (lwsp);

						token = new Token ((int) (word - input), n);
						token.Is8bit = !ascii;
						tokens.Add (token);

						encoded = false;
					}
				} else {
					// append the lwsp token
					if (lwsp != null)
						tokens.Add (lwsp);

					// append the non-atom token
					ascii = true;
					while (inptr < inend && !IsLwsp (*inptr) && !IsAtom (*inptr)) {
						ascii = ascii && IsAscii (*inptr);
						inptr++;
					}

					token = new Token ((int) (word - input), (int) (inptr - word));
					token.Is8bit = !ascii;
					tokens.Add (token);

					encoded = false;
				}
			}

			return tokens;
		}

		static unsafe List<Token> TokenizeText (byte* input, int length)
		{
			List<Token> tokens = new List<Token> ();
			byte* text, word, inptr = input;
			byte* inend = input + length;
			bool encoded = false;
			Token token = null;
			Token lwsp = null;
			bool ascii;
			int n;

			while (inptr < inend) {
				text = inptr;
				while (inptr < inend && IsLwsp (*inptr))
					inptr++;

				if (inptr > text)
					lwsp = new Token ((int) (inptr - input), (int) (inptr - text));
				else
					lwsp = null;

				if (inptr < inend) {
					word = inptr;
					ascii = true;

					if (EnableWorkarounds) {
						// Make an extra effort to detect and separate encoded-word
						// tokens that have been merged with other words.
						bool is_rfc2047 = false;

						if (inptr + 2 < inend && *inptr == '=' && *(inptr + 1) == '?') {
							inptr += 2;

							// skip past the charset (if one is even declared, sigh)
							while (inptr < inend && *inptr != '?') {
								ascii = ascii && IsAscii (*inptr);
								inptr++;
							}

							// sanity check encoding type
							if (inptr + 3 >= inend || *inptr != '?' || !IsBbQq (*(inptr + 1)) || *(inptr + 2) != '?') {
								ascii = true;
								goto non_rfc2047;
							}

							inptr += 3;

							// find the end of the rfc2047 encoded word token
							while (inptr + 2 < inend && *inptr != '?' && *(inptr + 1) != '=') {
								ascii = ascii && IsAscii (*inptr);
								inptr++;
							}

							if (inptr + 2 >= inend) {
								// didn't find an end marker...
								inptr = word + 2;
								ascii = true;

								goto non_rfc2047;
							}

							is_rfc2047 = true;
							inptr += 2;
						}

					non_rfc2047:
						if (!is_rfc2047) {
							// stop if we encounter a possible rfc2047 encoded
							// token even if it's inside another word, sigh.
							while (inptr < inend && !IsLwsp (*inptr)) {
								if (inptr + 2 < inend && *inptr == '=' && *(inptr + 1) == '?')
									break;

								ascii = ascii && IsAscii (*inptr);
								inptr++;
							}
						}
					} else {
						// find the end of a run of text...
						while (inptr < inend && !IsLwsp (*inptr)) {
							ascii = ascii && IsAscii (*inptr);
							inptr++;
						}
					}

					n = (int) (inptr - word);
					if (TryGetEncodedWordToken (input, word, n, out token)) {
						// rfc2047 states that you must ignore all whitespace between
						// encoded-word tokens
						if (!encoded && lwsp != null) {
							// previous token was not encoded, so preserve whitespace
							tokens.Add (lwsp);
						}

						tokens.Add (token);
						encoded = true;
					} else {
						// append the lwsp and atom tokens
						if (lwsp != null)
							tokens.Add (lwsp);

						token = new Token ((int) (word - input), n);
						token.Is8bit = !ascii;
						tokens.Add (token);

						encoded = false;
					}
				} else {
					// append the trailing lwsp token
					if (lwsp != null)
						tokens.Add (lwsp);

					break;
				}
			}

			return tokens;
		}

		static unsafe string DecodeTokens (List<Token> tokens, byte* input, int length)
		{
			throw new NotImplementedException ();
		}

		static unsafe string DecodePhrase (byte* phrase, int length)
		{
			var tokens = TokenizePhrase (phrase, length);

			return DecodeTokens (tokens, phrase, length);
		}

		public static string DecodePhrase (byte[] phrase)
		{
			if (phrase == null)
				throw new ArgumentNullException ("phrase");

			if (phrase.Length == 0)
				return string.Empty;

			unsafe {
				fixed (byte* inptr = phrase) {
					return DecodePhrase (inptr, phrase.Length);
				}
			}
		}

		static unsafe string DecodeText (byte* text, int length)
		{
			var tokens = TokenizeText (text, length);

			return DecodeTokens (tokens, text, length);
		}

		public static string DecodeText (byte[] text)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			if (text.Length == 0)
				return string.Empty;

			unsafe {
				fixed (byte* inptr = text) {
					return DecodeText (inptr, text.Length);
				}
			}
		}
	}
}
