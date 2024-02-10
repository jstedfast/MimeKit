//
// Rfc2047.cs
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
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using MimeKit.Encodings;

namespace MimeKit.Utils {
	/// <summary>
	/// Utility methods for encoding and decoding rfc2047 encoded-word tokens.
	/// </summary>
	/// <remarks>
	/// Utility methods for encoding and decoding rfc2047 encoded-word tokens.
	/// </remarks>
	public static class Rfc2047
	{
		readonly struct Token
		{
			const char SevenBit = '7';
			const char EightBit = '8';

			public readonly string CharsetCulture;
			public readonly int StartIndex;
			public readonly int Length;
			public readonly char Encoding;

#if REDUCE_TOKEN_SIZE
			// Note: .NET codepages range from ~34-65001 which all fit within a ushort, but we also need '-1'
			// to denote "unknown" and 0 to denote an unencoded ascii word.
			readonly ushort codePage;

			public int CodePage { get { return codePage == ushort.MaxValue ? -1 : codePage; } }
#else
			public readonly int CodePage;
#endif

			public bool Is8bit { get { return CodePage == 0 && Encoding == EightBit; } }

			public bool IsEncoded { get { return CodePage != 0; } }

			public Token (string charset, string culture, char encoding, int startIndex, int length)
			{
				CharsetCulture = string.IsNullOrEmpty (culture) ? charset : charset + "*" + culture;
#if REDUCE_TOKEN_SIZE
				int cp = CharsetUtils.GetCodePage (charset);
				codePage = cp < 0 ? ushort.MaxValue : (ushort) cp;
#else
				CodePage = CharsetUtils.GetCodePage (charset);
#endif
				StartIndex = startIndex;
				Length = length;
				Encoding = encoding;
			}

			public Token (int startIndex, int length, bool is8bit = false)
			{
				Encoding = is8bit ? EightBit : SevenBit;
				CharsetCulture = null;
				StartIndex = startIndex;
				Length = length;
#if REDUCE_TOKEN_SIZE
				codePage = 0;
#else
				CodePage = 0;
#endif
			}
		}

		struct CodePageCount
		{
			public readonly int CodePage;
			public int Count;

			public CodePageCount (int codepage)
			{
				CodePage = codepage;
				Count = 1;
			}
		}

		interface ITokenWriter : IDisposable
		{
			bool IgnoreWhitespaceBetweenEncodedWords { get; }
			void Write (ref ValueStringBuilder output, ref Token token);
			void Flush (ref ValueStringBuilder output);
		}

		class TokenDecoder : ITokenWriter
		{
			readonly ParserOptions options;
			readonly byte[] input, scratch;
			CodePageCount[] codepages;
			QuotedPrintableDecoder qp;
			Base64Decoder base64;
			IMimeDecoder decoder;
			int codepageIndex;
			int scratchLength;
			char encoding;
			int codepage;

			public TokenDecoder (ParserOptions options, byte[] input, byte[] scratch)
			{
				this.options = options;
				this.scratch = scratch;
				this.input = input;

				codepages = ArrayPool<CodePageCount>.Shared.Rent (16);
				base64 = null;
				qp = null;

				decoder = null;
				codepageIndex = 0;
				scratchLength = 0;
				encoding = '\0';
				codepage = 0;
			}

			public bool IgnoreWhitespaceBetweenEncodedWords {
				get { return true; }
			}

			void AddCodePage (int codepage)
			{
				for (int i = 0; i < codepageIndex; i++) {
					if (codepages[i].CodePage == codepage) {
						codepages[i].Count++;
						return;
					}
				}

				if (codepageIndex == codepages.Length) {
					var resized = ArrayPool<CodePageCount>.Shared.Rent (codepages.Length * 2);
					codepages.AsSpan ().CopyTo (resized);
					ArrayPool<CodePageCount>.Shared.Return (codepages);
					codepages = resized;
				}

				codepages[codepageIndex++] = new CodePageCount (codepage);
			}

			public unsafe void Write (ref ValueStringBuilder output, ref Token token)
			{
				if (decoder != null && (token.CodePage != codepage || char.ToUpperInvariant (token.Encoding) != encoding)) {
					// We've reached the end of a series of encoded-word tokens using identical charsets & encodings.
					//
					// In order to work around broken mailers, we need to combine the raw decoded content of runs of
					// identically encoded-word tokens before converting to unicode strings.
					Flush (ref output);
				}

				if (token.IsEncoded) {
					// Save encoded-word state so that we can treat consecutive encoded-word payloads with idential
					// charsets & encodings as one continuous block, thus allowing us to handle cases where a
					// hex-encoded triplet of a quoted-printable encoded payload is split between 2 or more
					// encoded-word tokens.
					encoding = char.ToUpperInvariant (token.Encoding);
					codepage = token.CodePage;
					if (encoding == 'Q')
						decoder = qp ??= new QuotedPrintableDecoder (true);
					else
						decoder = base64 ??= new Base64Decoder ();

					AddCodePage (codepage);

					fixed (byte* inbuf = input, outbuf = scratch) {
						int n = decoder.Decode (inbuf + token.StartIndex, token.Length, outbuf + scratchLength);
						scratchLength += n;
					}
				} else if (token.Is8bit) {
					// *sigh* I hate broken mailers...
					var unicode = CharsetUtils.ConvertToUnicode (options, input, token.StartIndex, token.Length, out int length);
					output.Append (unicode.AsSpan (0, length));
				} else {
					// pure 7bit ascii, a breath of fresh air...
					int endIndex = token.StartIndex + token.Length;
					for (int i = token.StartIndex; i < endIndex; i++)
						output.Append ((char) input[i]);
				}
			}

			public void Flush (ref ValueStringBuilder output)
			{
				// Reset any base64/quoted-printable decoder state
				decoder?.Reset ();
				decoder = null;

				if (scratchLength > 0) {
					// Convert any decoded encoded-word payloads into unicode and append to our 'decoded' buffer.
					var unicode = CharsetUtils.ConvertToUnicode (options, codepage, scratch, 0, scratchLength, out int length);
					output.Append (unicode.AsSpan (0, length));
					scratchLength = 0;
				}
			}

			public int GetMostCommonCodePage ()
			{
				int codepage = Encoding.UTF8.CodePage;
				int max = 0;

				for (int i = 0; i < codepageIndex; i++) {
					if (codepages[i].Count > max) {
						codepage = codepages[i].CodePage;
						max = codepages[i].Count;
					}
				}

				return codepage;
			}

			public void Dispose ()
			{
				ArrayPool<CodePageCount>.Shared.Return (codepages);
			}
		}

		class TokenFolder : ITokenWriter
		{
			readonly FormatOptions options;
			readonly byte[] input;
			bool firstToken;
			int lineLength;
			int lwsp, tab;

			public TokenFolder (FormatOptions options, string field, byte[] input)
			{
				this.options = options;
				this.input = input;

				lineLength = field.Length + 1;
				firstToken = true;
				lwsp = tab = 0;
			}

			public bool IgnoreWhitespaceBetweenEncodedWords {
				get { return false; }
			}

			public void Write (ref ValueStringBuilder output, ref Token token)
			{
				if (firstToken) {
					output.Append (' ');
					lineLength++;
				} else if (lineLength == 0) {
					output.Append (' ');
					lineLength = 1;
				}

				if (input[token.StartIndex].IsWhitespace ()) {
					for (int n = token.StartIndex; n < token.StartIndex + token.Length; n++) {
						if (input[n] == (byte) '\r')
							continue;

						lwsp = output.Length;
						if (input[n] == (byte) '\t')
							tab = output.Length;

						if (input[n] == (byte) '\n') {
							output.Append (options.NewLine);
							lwsp = tab = 0;
							lineLength = 0;
						} else {
							output.Append ((char) input[n]);
							lineLength++;
						}
					}

					firstToken = false;
				} else if (token.IsEncoded) {
					string charset = token.CharsetCulture;

					if (lineLength + token.Length + charset.Length + 7 > options.MaxLineLength) {
						if (tab != 0) {
							// tabs are the perfect breaking opportunity...
							output.Insert (tab, options.NewLine);
							lineLength = (lwsp - tab) + 1;
						} else if (lwsp != 0) {
							// break just before the last lwsp character
							output.Insert (lwsp, options.NewLine);
							lineLength = 1;
						} else if (lineLength > 1 && !firstToken) {
							// force a line break...
							output.Append (options.NewLine);
							output.Append (' ');
							lineLength = 1;
						}
					}

					// Note: if the encoded-word token is longer than the fold length, oh well...
					// it probably just means that we are folding a header written by a user-agent
					// with a different max line length than ours.

					output.Append ("=?");
					output.Append (charset);
					output.Append ('?');
					output.Append (token.Encoding);
					output.Append ('?');

					for (int n = token.StartIndex; n < token.StartIndex + token.Length; n++)
						output.Append ((char) input[n]);
					output.Append ("?=");

					lineLength += token.Length + charset.Length + 7;
					firstToken = false;
					lwsp = 0;
					tab = 0;
				} else if (lineLength + token.Length > options.MaxLineLength) {
					if (tab != 0) {
						// tabs are the perfect breaking opportunity...
						output.Insert (tab, options.NewLine);
						lineLength = (lwsp - tab) + 1;
					} else if (lwsp != 0) {
						// break just before the last lwsp character
						output.Insert (lwsp, options.NewLine);
						lineLength = 1;
					} else if (lineLength > 1 && !firstToken) {
						// force a line break...
						output.Append (options.NewLine);
						output.Append (' ');
						lineLength = 1;
					}

					if (token.Length >= options.MaxLineLength) {
						// the token is longer than the maximum allowable line length,
						// so we'll have to break it apart...
						for (int n = token.StartIndex; n < token.StartIndex + token.Length; n++) {
							if (lineLength >= options.MaxLineLength) {
								output.Append (options.NewLine);
								output.Append (' ');
								lineLength = 1;
							}

							output.Append ((char) input[n]);
							lineLength++;
						}
					} else {
						for (int n = token.StartIndex; n < token.StartIndex + token.Length; n++)
							output.Append ((char) input[n]);

						lineLength += token.Length;
					}

					firstToken = false;
					lwsp = 0;
					tab = 0;
				} else {
					for (int n = token.StartIndex; n < token.StartIndex + token.Length; n++)
						output.Append ((char) input[n]);

					lineLength += token.Length;
					firstToken = false;
					lwsp = 0;
					tab = 0;
				}
			}

			public void Flush (ref ValueStringBuilder output)
			{
				if (output.Length == 0 || output[output.Length - 1] != '\n')
					output.Append (options.NewLine);
			}

			public void Dispose ()
			{
			}
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool IsAscii (byte c)
		{
			return c < 128;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool IsAsciiAtom (byte c)
		{
			return c.IsAsciiAtom ();
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool IsAtom (byte c)
		{
			return c.IsAtom ();
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool IsBbQq (byte c)
		{
			return c == 'B' || c == 'b' || c == 'Q' || c == 'q';
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		static bool IsLwsp (byte c)
		{
			return c.IsWhitespace ();
		}

		static unsafe bool TryGetEncodedWordToken (byte* input, byte* word, int length, out Token token)
		{
			token = default;

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

			string charset, culture;

			using (var buffer = new ValueStringBuilder (32)) {
				// find the end of the charset name
				while (*inptr != '?' && *inptr != '*') {
					if (!IsAsciiAtom (*inptr))
						return false;

					buffer.Append ((char) *inptr);
					inptr++;
				}

				charset = buffer.ToString ();
			}

			if (*inptr == '*') {
				// we found a language code...
				inptr++;

				using (var buffer = new ValueStringBuilder (32)) {
					// find the end of the language code
					while (*inptr != '?') {
						if (!IsAsciiAtom (*inptr))
							return false;

						buffer.Append ((char) *inptr);
						inptr++;
					}

					culture = buffer.ToString ();
				}
			} else {
				culture = null;
			}

			// skip over the '?' to get to the encoding
			inptr++;

			char encoding;
			if (*inptr == 'B' || *inptr == 'b' || *inptr == 'Q' || *inptr == 'q') {
				encoding = (char) *inptr++;
			} else {
				return false;
			}

			if (*inptr != '?' || inptr == inend)
				return false;

			// skip over the '?' to get to the payload
			inptr++;

			int start = (int) (inptr - input);
			int len = (int) (inend - inptr);

			token = new Token (charset, culture, encoding, start, len);

			return true;
		}

		static unsafe void TokenizePhrase (ParserOptions options, ITokenWriter writer, ref ValueStringBuilder decoded, byte* inbuf, int startIndex, int length)
		{
			byte* text, word, inptr = inbuf + startIndex;
			byte* inend = inptr + length;
			var lwsp = new Token (0, 0);
			bool encoded = false;
			Token token;
			bool ascii;
			int n;

			while (inptr < inend) {
				text = inptr;
				while (inptr < inend && IsLwsp (*inptr))
					inptr++;

				lwsp = new Token ((int) (text - inbuf), (int) (inptr - text));

				word = inptr;
				ascii = true;
				if (inptr < inend && IsAsciiAtom (*inptr)) {
					if (options.Rfc2047ComplianceMode == RfcComplianceMode.Loose) {
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
							while (inptr + 2 < inend && !(*inptr == '?' && *(inptr + 1) == '=')) {
								ascii = ascii && IsAscii (*inptr);
								inptr++;
							}

							if (inptr + 2 > inend || *inptr != '?' || *(inptr + 1) != '=') {
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
								ascii = ascii && IsAscii (*inptr);
								inptr++;
							}
						}
					} else {
						// encoded-word tokens are atoms
						while (inptr < inend && IsAsciiAtom (*inptr)) {
							//ascii = ascii && IsAscii (*inptr);
							inptr++;
						}
					}

					n = (int) (inptr - word);
					if (TryGetEncodedWordToken (inbuf, word, n, out token)) {
						// rfc2047 states that you must ignore all whitespace between
						// encoded-word tokens
						if ((!encoded || !writer.IgnoreWhitespaceBetweenEncodedWords) && lwsp.Length > 0) {
							// previous token was not encoded, so preserve whitespace
							writer.Write (ref decoded, ref lwsp);
						}

						writer.Write (ref decoded, ref token);
						encoded = true;
					} else {
						// append the lwsp and atom tokens
						if (lwsp.Length > 0)
							writer.Write (ref decoded, ref lwsp);

						token = new Token ((int) (word - inbuf), n, !ascii);
						writer.Write (ref decoded, ref token);

						encoded = false;
					}
				} else {
					// append the lwsp token
					if (lwsp.Length > 0)
						writer.Write (ref decoded, ref lwsp);

					// append the non-ascii atom token
					ascii = true;
					while (inptr < inend && !IsLwsp (*inptr) && !IsAsciiAtom (*inptr)) {
						ascii = ascii && IsAscii (*inptr);
						inptr++;
					}

					token = new Token ((int) (word - inbuf), (int) (inptr - word), !ascii);
					writer.Write (ref decoded, ref token);

					encoded = false;
				}
			}

			writer.Flush (ref decoded);
		}

		static unsafe void TokenizeText (ParserOptions options, ITokenWriter writer, ref ValueStringBuilder output, byte* inbuf, int startIndex, int length)
		{
			byte* text, word, inptr = inbuf + startIndex;
			byte* inend = inptr + length;
			var lwsp = new Token (0, 0);
			bool encoded = false;
			Token token;
			bool ascii;
			int n;

			while (inptr < inend) {
				text = inptr;
				while (inptr < inend && IsLwsp (*inptr))
					inptr++;

				lwsp = new Token ((int) (text - inbuf), (int) (inptr - text));

				if (inptr < inend) {
					word = inptr;
					ascii = true;

					if (options.Rfc2047ComplianceMode == RfcComplianceMode.Loose) {
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
							while (inptr + 2 < inend && !(*inptr == '?' && *(inptr + 1) == '=')) {
								ascii = ascii && IsAscii (*inptr);
								inptr++;
							}

							if (inptr + 2 > inend || *inptr != '?' || *(inptr + 1) != '=') {
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
					if (TryGetEncodedWordToken (inbuf, word, n, out token)) {
						// rfc2047 states that you must ignore all whitespace between
						// encoded-word tokens
						if ((!encoded || !writer.IgnoreWhitespaceBetweenEncodedWords) && lwsp.Length > 0) {
							// previous token was not encoded, so preserve whitespace
							writer.Write (ref output, ref lwsp);
						}

						writer.Write (ref output, ref token);
						encoded = true;
					} else {
						// append the lwsp and atom tokens
						if (lwsp.Length > 0)
							writer.Write (ref output, ref lwsp);

						token = new Token ((int) (word - inbuf), n, !ascii);
						writer.Write (ref output, ref token);

						encoded = false;
					}
				} else {
					// append the trailing lwsp token
					if (lwsp.Length > 0)
						writer.Write (ref output, ref lwsp);

					break;
				}
			}

			writer.Flush (ref output);
		}

		internal static string DecodePhrase (ParserOptions options, byte[] phrase, int startIndex, int count, out int codepage)
		{
			codepage = Encoding.UTF8.CodePage;

			if (count == 0)
				return string.Empty;

			unsafe {
				fixed (byte* inbuf = phrase) {
					var scratch = count < 2048 ? ArrayPool<byte>.Shared.Rent (count) : new byte[count];
					var decoder = new TokenDecoder (options, phrase, scratch);
					var decoded = new ValueStringBuilder (count);

					try {
						TokenizePhrase (options, decoder, ref decoded, inbuf, startIndex, count);
						codepage = decoder.GetMostCommonCodePage ();
						return decoded.ToString ();
					} finally {
						if (count < 2048)
							ArrayPool<byte>.Shared.Return (scratch);
						decoder.Dispose ();
					}
				}
			}
		}

		/// <summary>
		/// Decode a phrase.
		/// </summary>
		/// <remarks>
		/// Decodes the phrase(s) starting at the given index and spanning across
		/// the specified number of bytes using the supplied parser options.
		/// </remarks>
		/// <returns>The decoded phrase.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="phrase">The phrase to decode.</param>
		/// <param name="startIndex">The starting index.</param>
		/// <param name="count">The number of bytes to decode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="phrase"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static string DecodePhrase (ParserOptions options, byte[] phrase, int startIndex, int count)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (phrase is null)
				throw new ArgumentNullException (nameof (phrase));

			if (startIndex < 0 || startIndex > phrase.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || startIndex + count > phrase.Length)
				throw new ArgumentOutOfRangeException (nameof (count));

			return DecodePhrase (options, phrase, startIndex, count, out _);
		}

		/// <summary>
		/// Decode a phrase.
		/// </summary>
		/// <remarks>
		/// Decodes the phrase(s) starting at the given index and spanning across
		/// the specified number of bytes using the default parser options.
		/// </remarks>
		/// <returns>The decoded phrase.</returns>
		/// <param name="phrase">The phrase to decode.</param>
		/// <param name="startIndex">The starting index.</param>
		/// <param name="count">The number of bytes to decode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="phrase"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static string DecodePhrase (byte[] phrase, int startIndex, int count)
		{
			return DecodePhrase (ParserOptions.Default, phrase, startIndex, count);
		}

		/// <summary>
		/// Decode a phrase.
		/// </summary>
		/// <remarks>
		/// Decodes the phrase(s) within the specified buffer using the supplied parser options.
		/// </remarks>
		/// <returns>The decoded phrase.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="phrase">The phrase to decode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="phrase"/> is <c>null</c>.</para>
		/// </exception>
		public static string DecodePhrase (ParserOptions options, byte[] phrase)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (phrase is null)
				throw new ArgumentNullException (nameof (phrase));

			return DecodePhrase (options, phrase, 0, phrase.Length);
		}

		/// <summary>
		/// Decode a phrase.
		/// </summary>
		/// <remarks>
		/// Decodes the phrase(s) within the specified buffer using the default parser options.
		/// </remarks>
		/// <returns>The decoded phrase.</returns>
		/// <param name="phrase">The phrase to decode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="phrase"/> is <c>null</c>.
		/// </exception>
		public static string DecodePhrase (byte[] phrase)
		{
			if (phrase is null)
				throw new ArgumentNullException (nameof (phrase));

			return DecodePhrase (phrase, 0, phrase.Length);
		}

		internal static string DecodeText (ParserOptions options, byte[] text, int startIndex, int count, out int codepage)
		{
			codepage = Encoding.UTF8.CodePage;

			if (count == 0)
				return string.Empty;

			unsafe {
				fixed (byte* inbuf = text) {
					var scratch = count < 2048 ? ArrayPool<byte>.Shared.Rent (count) : new byte[count];
					var decoder = new TokenDecoder (options, text, scratch);
					var decoded = new ValueStringBuilder (count);

					try {
						TokenizeText (options, decoder, ref decoded, inbuf, startIndex, count);
						codepage = decoder.GetMostCommonCodePage ();
						return decoded.ToString ();
					} finally {
						if (count < 2048)
							ArrayPool<byte>.Shared.Return (scratch);
						decoder.Dispose ();
					}
				}
			}
		}

		/// <summary>
		/// Decode unstructured text.
		/// </summary>
		/// <remarks>
		/// Decodes the unstructured text buffer starting at the given index and spanning
		/// across the specified number of bytes using the supplied parser options.
		/// </remarks>
		/// <returns>The decoded text.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="text">The text to decode.</param>
		/// <param name="startIndex">The starting index.</param>
		/// <param name="count">The number of bytes to decode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static string DecodeText (ParserOptions options, byte[] text, int startIndex, int count)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (text is null)
				throw new ArgumentNullException (nameof (text));

			if (startIndex < 0 || startIndex > text.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || startIndex + count > text.Length)
				throw new ArgumentOutOfRangeException (nameof (count));

			return DecodeText (options, text, startIndex, count, out _);
		}

		/// <summary>
		/// Decode unstructured text.
		/// </summary>
		/// <remarks>
		/// Decodes the unstructured text buffer starting at the given index and spanning
		/// across the specified number of bytes using the default parser options.
		/// </remarks>
		/// <returns>The decoded text.</returns>
		/// <param name="text">The text to decode.</param>
		/// <param name="startIndex">The starting index.</param>
		/// <param name="count">The number of bytes to decode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static string DecodeText (byte[] text, int startIndex, int count)
		{
			return DecodeText (ParserOptions.Default, text, startIndex, count);
		}

		/// <summary>
		/// Decode unstructured text.
		/// </summary>
		/// <remarks>
		/// Decodes the unstructured text buffer using the specified parser options.
		/// </remarks>
		/// <returns>The decoded text.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="text">The text to decode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		public static string DecodeText (ParserOptions options, byte[] text)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (text is null)
				throw new ArgumentNullException (nameof (text));

			return DecodeText (options, text, 0, text.Length);
		}

		/// <summary>
		/// Decode unstructured text.
		/// </summary>
		/// <remarks>
		/// Decodes the unstructured text buffer using the default parser options.
		/// </remarks>
		/// <returns>The decoded text.</returns>
		/// <param name="text">The text to decode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static string DecodeText (byte[] text)
		{
			if (text is null)
				throw new ArgumentNullException (nameof (text));

			return DecodeText (text, 0, text.Length);
		}

		internal static byte[] FoldUnstructuredHeader (FormatOptions options, string field, byte[] text)
		{
			unsafe {
				fixed (byte* inbuf = text) {
					var output = new ValueStringBuilder (text.Length + ((text.Length / options.MaxLineLength) * 2) + 2);
					var folder = new TokenFolder (options, field, text);

					try {
						TokenizeText (ParserOptions.Default, folder, ref output, inbuf, 0, text.Length);
						return Encoding.ASCII.GetBytes (output.ToString ());
					} finally {
						folder.Dispose ();
					}
				}
			}
		}

		static byte[] CharsetConvert (Encoding charset, char[] word, int length, out int converted)
		{
			var encoder = charset.GetEncoder ();
			int count = encoder.GetByteCount (word, 0, length, true);
			var encoded = new byte[count];

			converted = encoder.GetBytes (word, 0, length, encoded, 0, true);

			return encoded;
		}

		static ContentEncoding GetBestContentEncoding (byte[] text, int startIndex, int length)
		{
			int count = 0;

			for (int i = startIndex; i < startIndex + length; i++) {
				if (text[i] > 127)
					count++;
			}

			if ((double) count < (length * 0.17))
				return ContentEncoding.QuotedPrintable;

			return ContentEncoding.Base64;
		}

		static bool CharsetRequiresBase64 (Encoding encoding)
		{
			// https://tools.ietf.org/rfc/rfc1468.txt
			//
			// ISO-2022-JP may also be used in MIME Part 2 headers.  The "B"
			// encoding should be used with ISO-2022-JP text.
			return encoding.CodePage == 50220 || encoding.CodePage == 50222;
		}

		internal static int AppendEncodedWord (ref ValueStringBuilder builder, Encoding charset, string text, int startIndex, int length, QEncodeMode mode)
		{
			int startLength = builder.Length;
			var chars = new char[length];
			IMimeEncoder encoder;
			byte[] word, encoded;
			char encoding;
			int len;

			text.CopyTo (startIndex, chars, 0, length);

			try {
				word = CharsetConvert (charset, chars, length, out len);
			} catch {
				charset = Encoding.UTF8;
				word = CharsetConvert (charset, chars, length, out len);
			}

			if (CharsetRequiresBase64 (charset) || GetBestContentEncoding (word, 0, len) == ContentEncoding.Base64) {
				encoder = new Base64Encoder (true);
				encoding = 'b';
			} else {
				encoder = new QEncoder (mode);
				encoding = 'q';
			}

			encoded = ArrayPool<byte>.Shared.Rent (encoder.EstimateOutputLength (len));
			len = encoder.Flush (word, 0, len, encoded);

			builder.Append ("=?");
			builder.Append (CharsetUtils.GetMimeCharset (charset));
			builder.Append ('?');
			builder.Append (encoding);
			builder.Append ('?');

			for (int i = 0; i < len; i++)
				builder.Append ((char) encoded[i]);
			builder.Append ("?=");

			ArrayPool<byte>.Shared.Return (encoded);

			return builder.Length - startLength;
		}

		static void AppendQuoted (ref ValueStringBuilder str, string text, int startIndex, int length)
		{
			int lastIndex = startIndex + length;
			char c;

			str.Append ('"');

			for (int i = startIndex; i < lastIndex; i++) {
				c = text[i];

				if (c == '"' || c == '\\')
					str.Append ('\\');

				str.Append (c);
			}

			str.Append ('"');
		}

		enum WordType {
			Atom,
			QuotedString,
			EncodedWord
		}

		enum WordEncoding {
			Ascii,
			Latin1,
			UserSpecified
		}

		class Word {
			//public readonly string Text;
			public WordType Type;
			public int StartIndex;
			public int CharCount;
			public WordEncoding Encoding;
			public int ByteCount;
			public int EncodeCount;
			public int QuotedPairs;
			public bool IsQuotedStart;
			public bool IsQuotedEnd;
			public bool IsCommentStart;
			public bool IsCommentEnd;

			public Word (string text, int startIndex)
			{
				//Text = text;
				StartIndex = startIndex;
			}

			public void CopyTo (Word word)
			{
				word.IsQuotedStart = IsQuotedStart;
				word.IsQuotedEnd = IsQuotedEnd;
				word.IsCommentStart = IsCommentStart;
				word.IsCommentEnd = IsCommentEnd;
				word.EncodeCount = EncodeCount;
				word.QuotedPairs = QuotedPairs;
				word.StartIndex = StartIndex;
				word.CharCount = CharCount;
				word.ByteCount = ByteCount;
				word.Encoding = Encoding;
				word.Type = Type;
			}

			//public override string ToString ()
			//{
			//	return $"{Type}: {Text.Substring (StartIndex, CharCount)}";
			//}
		}

		static bool IsAtom (char c)
		{
			return ((byte) c).IsAtom ();
		}

		static bool IsBlank (char c)
		{
			return c == ' ' || c == '\t';
		}

		static bool IsCtrl (char c)
		{
			return ((byte) c).IsCtrl ();
		}

		static int EstimateEncodedWordLength (string charset, int byteCount, int encodeCount)
		{
			int length = charset.Length + 7;

			if ((double) encodeCount < (byteCount * 0.17)) {
				// quoted-printable encoding
				return length + (byteCount - encodeCount) + (encodeCount * 3);
			}

			// base64 encoding
			return length + ((byteCount + 2) / 3) * 4;
		}

		static int EstimateEncodedWordLength (Encoding charset, int byteCount, int encodeCount)
		{
			return EstimateEncodedWordLength (CharsetUtils.GetMimeCharset (charset), byteCount, encodeCount);
		}

		static bool ExceedsMaxLineLength (FormatOptions options, Encoding charset, Word word)
		{
			int length;

			switch (word.Type) {
			case WordType.EncodedWord:
				switch (word.Encoding) {
				case WordEncoding.Latin1:
					length = EstimateEncodedWordLength ("iso-8859-1", word.ByteCount, word.EncodeCount);
					break;
				case WordEncoding.Ascii:
					length = EstimateEncodedWordLength ("us-ascii", word.ByteCount, word.EncodeCount);
					break;
				default:
					length = EstimateEncodedWordLength (charset, word.ByteCount, word.EncodeCount);
					break;
				}
				break;
			case WordType.QuotedString:
				length = word.ByteCount + word.QuotedPairs + 2;
				break;
			default:
				length = word.ByteCount;
				break;
			}

			return length + 1 >= options.MaxLineLength;
		}

		static List<Word> GetRfc822Words (FormatOptions options, Encoding charset, string text, int startIndex, int count, bool phrase)
		{
			var encoder = charset.GetEncoder ();
			var saved = new Word (text, startIndex);
			var word = new Word (text, startIndex);
			var words = new List<Word> ();
			int endIndex, nchars, n, i;
			var chars = new char[2];
			int commentDepth = 0;
			var escaped = false;
			var quoted = false;
			char c;

			endIndex = startIndex + count;
			i = startIndex;

			while (i < endIndex) {
				c = text[i++];

				if (!quoted && commentDepth == 0 && IsBlank (c)) {
					if (word.ByteCount > 0) {
						words.Add (word);
						word = new Word (text, i);
					} else {
						word.StartIndex = i;
					}
				} else {
					// save state in case adding this character exceeds the max line length
					word.CopyTo (saved);

					if (c < 127) {
						if (IsCtrl (c)) {
							word.Encoding = (WordEncoding) Math.Max ((int) word.Encoding, (int) WordEncoding.Latin1);
							word.Type = WordType.EncodedWord;
							word.EncodeCount++;
						} else if (phrase && !IsAtom (c)) {
							// phrases can have quoted strings
							if (word.Type == WordType.Atom)
								word.Type = WordType.QuotedString;

							if (c == '\\') {
								word.QuotedPairs++;
								escaped = !escaped;
							} else if (c == '"') {
								if (!escaped) {
									quoted = !quoted;
									if (quoted) {
										word.IsQuotedStart = true;
									} else {
										word.IsQuotedEnd = true;
									}
								}
								word.QuotedPairs++;
								escaped = false;
							} else if (!quoted) {
								if (c == '(') {
									word.IsCommentStart = commentDepth == 0;
									commentDepth++;
								} else if (c == ')' && commentDepth > 0) {
									commentDepth--;
									word.IsCommentEnd = commentDepth == 0;
								}
							} else {
								escaped = false;
							}
						}

						word.ByteCount++;
						word.CharCount++;
						nchars = 1;
					} else if (c < 256) {
						// iso-8859-1
						word.Encoding = (WordEncoding) Math.Max ((int) word.Encoding, (int) WordEncoding.Latin1);
						word.Type = WordType.EncodedWord;
						word.EncodeCount++;
						word.ByteCount++;
						word.CharCount++;
						nchars = 1;
					} else {
						if (char.IsSurrogatePair (text, i - 1)) {
							chars[1] = text[i++];
							nchars = 2;
						} else {
							nchars = 1;
						}

						chars[0] = c;

						try {
							n = encoder.GetByteCount (chars, 0, nchars, true);
						} catch {
							n = 3 * nchars;
						}

						word.Encoding = WordEncoding.UserSpecified;
						word.Type = WordType.EncodedWord;
						word.CharCount += nchars;
						word.EncodeCount += n;
						word.ByteCount += n;
					}

					if (ExceedsMaxLineLength (options, charset, word)) {
						// restore our previous state
						saved.CopyTo (word);
						i -= nchars;

						// Note: if the word is longer than what we can fit on
						// one line, then we need to encode it.
						if (word.Type == WordType.Atom) {
							word.Type = WordType.EncodedWord;

							// in order to fit this long atom under MaxLineLength, we need to
							// account for the added length of =?us-ascii?q?...?=
							n = "us-ascii".Length + 7;
							word.CharCount -= n;
							word.ByteCount -= n;
							i -= n;
						}

						words.Add (word);

						saved.Type = word.Type;
						word = new Word (text, i) {
							// Note: the word-type needs to be preserved when breaking long words.
							Type = saved.Type
						};
					} else if (word.IsQuotedEnd || word.IsCommentEnd) {
						if (word.Type == WordType.EncodedWord) {
							if (word.IsQuotedEnd && !word.IsQuotedStart) {
								for (int j = words.Count - 1; j >= 0; j--) {
									words[j].Type = WordType.EncodedWord;
									if (words[j].IsQuotedStart)
										break;
								}
							} else if (word.IsCommentEnd && !word.IsCommentStart) {
								for (int j = words.Count - 1; j >= 0; j--) {
									words[j].Type = WordType.EncodedWord;
									if (words[j].IsCommentStart)
										break;
								}
							}
						}

						// End the word here.
						words.Add (word);

						word = new Word (text, i);
					}
				}
			}

			if (word.ByteCount > 0)
				words.Add (word);

			return words;
		}

		static bool ShouldMergeWords (FormatOptions options, Encoding charset, List<Word> words, Word word, int i)
		{
			Word next = words[i];

			int lwspCount = next.StartIndex - (word.StartIndex + word.CharCount);
			int length = word.ByteCount + lwspCount + next.ByteCount;
			int encoded = word.EncodeCount + next.EncodeCount;

			switch (word.Type) {
			case WordType.Atom:
				if (next.Type == WordType.EncodedWord)
					return false;

				return length + 1 < options.MaxLineLength;
			case WordType.QuotedString:
				if (next.Type == WordType.EncodedWord)
					return false;

				// Quoted strings can always be combined with atoms or other quoted strings.
				return true;
			case WordType.EncodedWord:
				if (next.Type == WordType.Atom) {
					// whether we merge or not is dependent upon:
					// 1. the number of atoms in a row after 'word'
					// 2. if there is another encoded-word after
					//    the string of atoms.
					bool merge = false;
					int natoms = 0;

					for (int j = i + 1; j < words.Count && natoms < 3; j++) {
						if (words[j].Type != WordType.Atom) {
							merge = true;
							break;
						}

						natoms++;
					}

					// if all the words after the encoded-word are atoms, don't merge
					if (!merge)
						return false;
				}

				// avoid merging with qstrings
				if (next.Type == WordType.QuotedString)
					return false;

				switch ((WordEncoding) Math.Max ((int) word.Encoding, (int) next.Encoding)) {
				case WordEncoding.Latin1:
					length = EstimateEncodedWordLength ("iso-8859-1", length, encoded);
					break;
				case WordEncoding.Ascii:
					length = EstimateEncodedWordLength ("us-ascii", length, encoded);
					break;
				default:
					length = EstimateEncodedWordLength (charset, length, encoded);
					break;
				}

				return length + 1 < options.MaxLineLength;
			default:
				return false;
			}
		}

		static void Merge (Word word, Word next)
		{
			// the resulting word is the max of the 2 types
			int lwspCount = next.StartIndex - (word.StartIndex + word.CharCount);

			word.Type = (WordType) Math.Max ((int) word.Type, (int) next.Type);
			word.CharCount = (next.StartIndex + next.CharCount) - word.StartIndex;
			word.ByteCount = word.ByteCount + lwspCount + next.ByteCount;
			word.Encoding = (WordEncoding) Math.Max ((int) word.Encoding, (int) next.Encoding);
			word.EncodeCount += next.EncodeCount;
			word.QuotedPairs += next.QuotedPairs;
		}

		static List<Word> MergeAdjacent (FormatOptions options, Encoding charset, List<Word> words)
		{
			int lwspCount, encoded, quoted, byteCount, length;
			var merged = new List<Word> ();
			WordEncoding encoding;
			Word word, next;

			word = words[0];
			merged.Add (word);

			// merge qstrings with adjacent qstrings and encoded-words with adjacent encoded-words
			for (int i = 1; i < words.Count; i++) {
				next = words[i];

				if (word.Type != WordType.Atom && word.Type == next.Type) {
					encoding = (WordEncoding) Math.Max ((int) word.Encoding, (int) next.Encoding);
					lwspCount = next.StartIndex - (word.StartIndex + word.CharCount);
					byteCount = word.ByteCount + lwspCount + next.ByteCount;
					encoded = word.EncodeCount + next.EncodeCount;
					quoted = word.QuotedPairs + next.QuotedPairs;

					if (word.Type == WordType.EncodedWord) {
						switch (encoding) {
						case WordEncoding.Latin1:
							length = EstimateEncodedWordLength ("iso-8859-1", byteCount, encoded);
							break;
						case WordEncoding.Ascii:
							length = EstimateEncodedWordLength ("us-ascii", byteCount, encoded);
							break;
						default:
							length = EstimateEncodedWordLength (charset, byteCount, encoded);
							break;
						}
					} else {
						length = byteCount + quoted + 2;
					}

					if (length + 1 < options.MaxLineLength) {
						word.CharCount = (next.StartIndex + next.CharCount) - word.StartIndex;
						word.ByteCount = byteCount;
						word.EncodeCount = encoded;
						word.QuotedPairs = quoted;
						word.Encoding = encoding;
						continue;
					}
				}

				merged.Add (next);
				word = next;
			}

			return merged;
		}

		static List<Word> Merge (FormatOptions options, Encoding charset, List<Word> words)
		{
			if (words.Count < 2)
				return words;

			List<Word> merged;
			Word word, next;

			merged = MergeAdjacent (options, charset, words);
			words = merged;

			merged = new List<Word> ();
			word = words[0];
			merged.Add (word);

			// second pass: now merge atoms with the other words
			for (int i = 1; i < words.Count; i++) {
				next = words[i];

				if (ShouldMergeWords (options, charset, words, word, i)) {
					Merge (word, next);
				} else {
					merged.Add (next);
					word = next;
				}
			}

			return merged;
		}

		enum EncodeType
		{
			Phrase,
			Text,
			Comment
		}

		static void Encode (ref ValueStringBuilder builder, FormatOptions options, Encoding charset, string text, int startIndex, int count, EncodeType type)
		{
			var mode = type == EncodeType.Phrase ? QEncodeMode.Phrase : QEncodeMode.Text;
			var words = GetRfc822Words (options, charset, text, startIndex, count, type == EncodeType.Phrase);
			int start, length;
			Word prev = null;

			words = Merge (options, charset, words);

			if (!options.AllowMixedHeaderCharsets) {
				for (int i = 0; i < words.Count; i++) {
					if (words[i].Type == WordType.EncodedWord)
						words[i].Encoding = WordEncoding.UserSpecified;
				}
			}

			if (type == EncodeType.Comment)
				builder.Append ('(');

			foreach (var word in words) {
				// append the correct number of spaces between words...
				if (prev != null && !(prev.Type == WordType.EncodedWord && word.Type == WordType.EncodedWord)) {
					start = prev.StartIndex + prev.CharCount;
					length = word.StartIndex - start;
					builder.Append (text.AsSpan (start, length));
				}

				switch (word.Type) {
				case WordType.Atom:
					builder.Append (text.AsSpan (word.StartIndex, word.CharCount));
					break;
				case WordType.QuotedString:
					AppendQuoted (ref builder, text, word.StartIndex, word.CharCount);
					break;
				case WordType.EncodedWord:
					if (prev != null && prev.Type == WordType.EncodedWord) {
						// include the whitespace between these 2 words in the
						// resulting rfc2047 encoded-word.
						start = prev.StartIndex + prev.CharCount;
						length = (word.StartIndex + word.CharCount) - start;

						builder.Append (type == EncodeType.Phrase ? '\t' : ' ');
					} else {
						start = word.StartIndex;
						length = word.CharCount;
					}

					switch (word.Encoding) {
					case WordEncoding.Ascii:
						AppendEncodedWord (ref builder, Encoding.ASCII, text, start, length, mode);
						break;
					case WordEncoding.Latin1:
						AppendEncodedWord (ref builder, CharsetUtils.Latin1, text, start, length, mode);
						break;
					default: // custom charset
						AppendEncodedWord (ref builder, charset, text, start, length, mode);
						break;
					}
					break;
				}

				prev = word;
			}

			if (type == EncodeType.Comment)
				builder.Append (')');
		}

		static byte[] AsByteArray (ref ValueStringBuilder builder)
		{
			var encoded = new byte[builder.Length];

#if NET5_0_OR_GREATER
			Encoding.ASCII.GetBytes (builder.AsSpan (), encoded);
#else
			for (int i = 0; i < builder.Length; i++)
				encoded[i] = (byte) builder[i];
#endif

			return encoded;
		}

		static byte[] EncodeAsBytes (FormatOptions options, Encoding charset, string text, int startIndex, int count, EncodeType type)
		{
			var builder = new ValueStringBuilder (count * 4);
			Encode (ref builder, options, charset, text, startIndex, count, type);
			var encoded = AsByteArray (ref builder);
			builder.Dispose ();
			return encoded;
		}

		static string EncodeAsString (FormatOptions options, Encoding charset, string text, int startIndex, int count, EncodeType type)
		{
			var builder = new ValueStringBuilder (count * 4);
			Encode (ref builder, options, charset, text, startIndex, count, type);
			var encoded = builder.ToString ();
			builder.Dispose ();
			return encoded;
		}

		static void ValidateArguments (FormatOptions options, Encoding charset, string text, string textArgName)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			if (charset is null)
				throw new ArgumentNullException (nameof (charset));

			if (text is null)
				throw new ArgumentNullException (textArgName);
		}

		static void ValidateArguments (FormatOptions options, Encoding charset, string text, string textArgName, int startIndex, int count)
		{
			ValidateArguments (options, charset, text, textArgName);

			if (startIndex < 0 || startIndex > text.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			if (count < 0 || count > (text.Length - startIndex))
				throw new ArgumentOutOfRangeException (nameof (count));
		}

		/// <summary>
		/// Encode comment text.
		/// </summary>
		/// <remarks>
		/// Encodes the comment text and wraps the result in parenthesis according to the rules of rfc2047
		/// using the specified charset encoding and formatting options.
		/// </remarks>
		/// <returns>The encoded text.</returns>
		/// <param name="options">The formatting options</param>
		/// <param name="charset">The charset encoding.</param>
		/// <param name="text">The text to encode.</param>
		/// <param name="startIndex">The starting index of the phrase to encode.</param>
		/// <param name="count">The number of characters to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		internal static string EncodeComment (FormatOptions options, Encoding charset, string text, int startIndex, int count)
		{
			//ValidateArguments (options, charset, text, nameof (text), startIndex, count);

			return EncodeAsString (options, charset, text, startIndex, count, EncodeType.Comment);
		}

		/// <summary>
		/// Encode a phrase.
		/// </summary>
		/// <remarks>
		/// Encodes the phrase according to the rules of rfc2047 using
		/// the specified charset encoding and formatting options.
		/// </remarks>
		/// <returns>The encoded phrase.</returns>
		/// <param name="options">The formatting options</param>
		/// <param name="charset">The charset encoding.</param>
		/// <param name="phrase">The phrase to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="phrase"/> is <c>null</c>.</para>
		/// </exception>
		internal static string EncodePhraseAsString (FormatOptions options, Encoding charset, string phrase)
		{
			ValidateArguments (options, charset, phrase, nameof (phrase));

			return EncodeAsString (options, charset, phrase, 0, phrase.Length, EncodeType.Phrase);
		}

		/// <summary>
		/// Encode a phrase.
		/// </summary>
		/// <remarks>
		/// Encodes the phrase according to the rules of rfc2047 using
		/// the specified charset encoding and formatting options.
		/// </remarks>
		/// <returns>The encoded phrase.</returns>
		/// <param name="options">The formatting options</param>
		/// <param name="charset">The charset encoding.</param>
		/// <param name="phrase">The phrase to encode.</param>
		/// <param name="startIndex">The starting index of the phrase to encode.</param>
		/// <param name="count">The number of characters to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="phrase"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static byte[] EncodePhrase (FormatOptions options, Encoding charset, string phrase, int startIndex, int count)
		{
			ValidateArguments (options, charset, phrase, nameof (phrase), startIndex, count);

			return EncodeAsBytes (options, charset, phrase, startIndex, count, EncodeType.Phrase);
		}

		/// <summary>
		/// Encode a phrase.
		/// </summary>
		/// <remarks>
		/// Encodes the phrase according to the rules of rfc2047 using
		/// the specified charset encoding and formatting options.
		/// </remarks>
		/// <returns>The encoded phrase.</returns>
		/// <param name="charset">The charset encoding.</param>
		/// <param name="phrase">The phrase to encode.</param>
		/// <param name="startIndex">The starting index of the phrase to encode.</param>
		/// <param name="count">The number of characters to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="phrase"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static byte[] EncodePhrase (Encoding charset, string phrase, int startIndex, int count)
		{
			return EncodePhrase (FormatOptions.Default, charset, phrase, startIndex, count);
		}

		/// <summary>
		/// Encode a phrase.
		/// </summary>
		/// <remarks>
		/// Encodes the phrase according to the rules of rfc2047 using
		/// the specified charset encoding and formatting options.
		/// </remarks>
		/// <returns>The encoded phrase.</returns>
		/// <param name="options">The formatting options</param>
		/// <param name="charset">The charset encoding.</param>
		/// <param name="phrase">The phrase to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="phrase"/> is <c>null</c>.</para>
		/// </exception>
		public static byte[] EncodePhrase (FormatOptions options, Encoding charset, string phrase)
		{
			ValidateArguments (options, charset, phrase, nameof (phrase));

			return EncodeAsBytes (options, charset, phrase, 0, phrase.Length, EncodeType.Phrase);
		}

		/// <summary>
		/// Encode a phrase.
		/// </summary>
		/// <remarks>
		/// Encodes the phrase according to the rules of rfc2047 using
		/// the specified charset encoding.
		/// </remarks>
		/// <returns>The encoded phrase.</returns>
		/// <param name="charset">The charset encoding.</param>
		/// <param name="phrase">The phrase to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="phrase"/> is <c>null</c>.</para>
		/// </exception>
		public static byte[] EncodePhrase (Encoding charset, string phrase)
		{
			return EncodePhrase (FormatOptions.Default, charset, phrase);
		}

		/// <summary>
		/// Encode unstructured text.
		/// </summary>
		/// <remarks>
		/// Encodes the unstructured text according to the rules of rfc2047
		/// using the specified charset encoding and formatting options.
		/// </remarks>
		/// <returns>The encoded text.</returns>
		/// <param name="options">The formatting options</param>
		/// <param name="charset">The charset encoding.</param>
		/// <param name="text">The text to encode.</param>
		/// <param name="startIndex">The starting index of the phrase to encode.</param>
		/// <param name="count">The number of characters to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static byte[] EncodeText (FormatOptions options, Encoding charset, string text, int startIndex, int count)
		{
			ValidateArguments (options, charset, text, nameof (text), startIndex, count);

			return EncodeAsBytes (options, charset, text, startIndex, count, EncodeType.Text);
		}

		/// <summary>
		/// Encode unstructured text.
		/// </summary>
		/// <remarks>
		/// Encodes the unstructured text according to the rules of rfc2047
		/// using the specified charset encoding and formatting options.
		/// </remarks>
		/// <returns>The encoded text.</returns>
		/// <param name="charset">The charset encoding.</param>
		/// <param name="text">The text to encode.</param>
		/// <param name="startIndex">The starting index of the phrase to encode.</param>
		/// <param name="count">The number of characters to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> and <paramref name="count"/> do not specify
		/// a valid range in the byte array.
		/// </exception>
		public static byte[] EncodeText (Encoding charset, string text, int startIndex, int count)
		{
			return EncodeText (FormatOptions.Default, charset, text, startIndex, count);
		}

		/// <summary>
		/// Encode unstructured text.
		/// </summary>
		/// <remarks>
		/// Encodes the unstructured text according to the rules of rfc2047
		/// using the specified charset encoding and formatting options.
		/// </remarks>
		/// <returns>The encoded text.</returns>
		/// <param name="options">The formatting options</param>
		/// <param name="charset">The charset encoding.</param>
		/// <param name="text">The text to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		public static byte[] EncodeText (FormatOptions options, Encoding charset, string text)
		{
			ValidateArguments (options, charset, text, nameof (text));

			return EncodeAsBytes (options, charset, text, 0, text.Length, EncodeType.Text);
		}

		/// <summary>
		/// Encode unstructured text.
		/// </summary>
		/// <remarks>
		/// Encodes the unstructured text according to the rules of rfc2047
		/// using the specified charset encoding.
		/// </remarks>
		/// <returns>The encoded text.</returns>
		/// <param name="charset">The charset encoding.</param>
		/// <param name="text">The text to encode.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="charset"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="text"/> is <c>null</c>.</para>
		/// </exception>
		public static byte[] EncodeText (Encoding charset, string text)
		{
			return EncodeText (FormatOptions.Default, charset, text);
		}
	}
}
