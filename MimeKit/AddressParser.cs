//
// AddressParser.cs
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
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using MimeKit.Utils;

namespace MimeKit {
	// Owns a complete input slice. Group and list parsing share the same cursor.
	internal struct AddressParser<TInput> where TInput : struct, IAddressParserInput
	{
		readonly ParserOptions options;
		readonly TInput text;
		readonly int endIndex;
		Encoding? cachedEncoding;
		int cachedCodepage;
		int index;

		internal AddressParser (ParserOptions options, TInput text, int startIndex, int endIndex)
		{
			this.options = options;
			this.text = text;
			this.endIndex = endIndex;
			cachedEncoding = null;
			cachedCodepage = 0;
			index = startIndex;
		}

		internal int Position => index;

		internal bool TryParseCompleteAddress (AddressParserFlags flags, [NotNullWhen (true)] out InternetAddress? address)
		{
			bool throwOnError = (flags & AddressParserFlags.ThrowOnError) != 0;

			if (!TryParseAddress (flags, 0, out address))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError) || index != endIndex) {
				if (throwOnError) {
					int offset = text.GetByteOffset (index);
					string message = (flags & AddressParserFlags.TryParse) == AddressParserFlags.TryParse
						? string.Format (CultureInfo.InvariantCulture, "Unexpected '{0}' token at offset {1}", text.GetErrorCharacter (index), offset)
						: string.Format (CultureInfo.InvariantCulture, "Unexpected token at offset {0}", offset);

					throw new ParseException (message, offset, offset);
				}

				address = null;
				return false;
			}

			return true;
		}

		bool SkipQuoted (bool throwOnError)
		{
			// From rfc5322:
			//
			// qcontent        =   qtext / quoted-pair
			//
			// qtext           =   %d33 /             ; Printable US-ASCII
			//                     %d35-91 /          ; characters not including
			//                     %d93-126 /         ; "\" or the quote character
			//                     obs-qtext
			//
			// obs-qtext       =   <any CHAR excepting <">,     ; => may be folded
			//                     "\" & CR, and including
			//                     linear-white-space>
			//
			// quoted-pair     =   ("\" (VCHAR / WSP)) / obs-qp
			//
			// VCHAR           =  %x21-7E             ; visible (printing) characters
			//
			// WSP             =  SP / HTAB           ; white space
			//
			// obs-qp          =   "\" (%d0 / obs-NO-WS-CTL / LF / CR)
			//
			// obs-NO-WS-CTL   =   %d1-8 /            ; US-ASCII control
			//                     %d11 /             ;  characters that do not
			//                     %d12 /             ;  include the carriage
			//                     %d14-31 /          ;  return, line feed, and
			//                     %d127              ;  white space characters
			int startIndex = index;
			bool escaped = false;

			// skip over leading '"'
			index++;

			while (index < endIndex) {
				char c = text[index];

				if (c == (byte) '\\') {
					escaped = !escaped;
				} else if (!escaped) {
					if (c == (byte) '"')
						break;

					// Note: the qtext definition from rfc5322 accepts %d33, %d35-91, and %d93-126 and so you
					// might expect that we limit the characters to those ranges, but rfc6532 extends the qtext
					// definition to allow UTF-8 characters and so we need to allow for that as well. In addition
					// to that, obs-qtext also explicitly allows linear whitespace (SPACE %d32 and HTAB %d09), so
					// the only characters that are not allowed are the control characters (%d0-8 and %d10-31),
					// the double-quote character (%d34), and the delete character (%d127).
					if ((c < 32 && c != 9) || c == 127) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid control character in quoted-string token at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

						return false;
					}
				} else {
					// Note: We are going to make the decision to not support any escaped ctrl characters even
					// though they are allowed by obs-qp. They are just too dangerous.
					if ((c < 32 && c != 9) || c == 127) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid quoted-pair in quoted-string token at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

						return false;
					}

					escaped = false;
				}

				index++;
			}

			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete quoted-string token at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

				return false;
			}

			// skip over the closing '"'
			index++;

			return true;
		}

		bool TryParseLocalPart (RfcComplianceMode compliance, bool skipTrailingCfws, bool throwOnError, [NotNullWhen (true)] out string? localpart)
		{
			using var token = new ValueStringBuilder (128);
			int startIndex = index;

			localpart = null;

			do {
				bool escapedAt = false;
				int start = index;

				if (text[index] == (byte) '"') {
					if (!SkipQuoted (throwOnError))
						return false;
				} else if (text.IsAtom (index)) {
					if (!ParseUtils.SkipAtom (text, ref index, endIndex))
						return false;

					if (compliance == RfcComplianceMode.Looser) {
						// Allow local-parts that include escaped '@' symbols.
						// See https://github.com/jstedfast/MimeKit/issues/1043 for details.
						while (index + 1 < endIndex && text[index] == (byte) '\\' && text[index + 1] == (byte) '@') {
							// track that we've encountered an escaped @ symbol
							escapedAt = true;

							// skip over the '\\' and '@' characters
							index += 2;

							if (!ParseUtils.SkipAtom (text, ref index, endIndex))
								break;
						}
					}
				} else {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid local-part at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

					return false;
				}

				string word;

				try {
					word = text.GetUtf8String (start, index - start);
				} catch (DecoderFallbackException ex) {
					if (compliance == RfcComplianceMode.Strict) {
						if (throwOnError)
							throw new ParseException ("Internationalized local-part tokens may only contain UTF-8 characters.", text.GetByteOffset (start), text.GetByteOffset (start), ex);

						return false;
					}

					word = text.GetLatin1String (start, index - start);
				}

				if (escapedAt)
					word = word.Replace ("\\@", "%40");

				token.Append (word);

				int cfws = index;
				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex || text[index] != (byte) '.') {
					if (!skipTrailingCfws)
						index = cfws;
					break;
				}

				do {
					token.Append ('.');
					index++;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					if (index >= endIndex) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete local-part at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

						return false;
					}
				} while (compliance == RfcComplianceMode.Looser && text[index] == (byte) '.');

				if (compliance == RfcComplianceMode.Looser && (index >= endIndex || text[index] == (byte) '@'))
					break;
			} while (true);

			localpart = token.ToString ();

			return true;
		}

		// Decodes the display name found in a trailing comment (e.g. "user@example.com (Display Name)"). Tabs are
		// converted into spaces for consistency with how display names in a phrase are handled by MimeUtils.Unquote().
		string DecodeCommentName (int startIndex, int length)
		{
			return text.DecodeComment (options, startIndex, length);
		}

		static ReadOnlySpan<byte> CommaGreaterThanOrSemiColon => ",>;"u8;

		bool TryScanAsciiDotAtom (ref int cursor, ReadOnlySpan<byte> sentinels)
		{
			do {
				int startIndex = cursor;

				while (cursor < endIndex && text.IsAsciiAtom (cursor))
					cursor++;

				if (cursor == startIndex)
					return false;

				if (cursor == endIndex || text[cursor] != '.')
					return true;

				cursor++;

				if (cursor < endIndex && ParseUtils.IsSentinel (text[cursor], sentinels))
					return false;
			} while (true);
		}

		bool TryParseContiguousAddrspec (ReadOnlySpan<byte> sentinels, [NotNullWhen (true)] out string? addrspec, out int at)
		{
			int cursor = index;

			addrspec = null;
			at = -1;

			if (!TryScanAsciiDotAtom (ref cursor, ReadOnlySpan<byte>.Empty) || cursor == endIndex || text[cursor] != '@' || ParseUtils.IsSentinel ('@', sentinels))
				return false;

			int atIndex = cursor++;

			if (!TryScanAsciiDotAtom (ref cursor, sentinels))
				return false;

			// CFWS may hide another domain label. Leave it and all repairs to the full grammar.
			if (cursor < endIndex && !ParseUtils.IsSentinel (text[cursor], sentinels))
				return false;

			addrspec = text.GetUtf8String (index, cursor - index);
			at = atIndex - index;
			index = cursor;
			return true;
		}

		internal bool TryParseAddrspec (ReadOnlySpan<byte> sentinels, RfcComplianceMode compliance, bool throwOnError, [NotNullWhen (true)] out string? addrspec, out int at)
		{
			if (TryParseContiguousAddrspec (sentinels, out addrspec, out at))
				return true;

			int startIndex = index;

			addrspec = null;
			at = -1;

			if (!TryParseLocalPart (compliance, true, throwOnError, out var localpart))
				return false;

			if (index >= endIndex || ParseUtils.IsSentinel (text[index], sentinels)) {
				addrspec = localpart;
				return true;
			}

			if (text[index] != (byte) '@') {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid addr-spec token at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

				return false;
			}

			index++;
			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete addr-spec token at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

				return false;
			}

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete addr-spec token at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

				return false;
			}

			if (!ParseUtils.TryParseDomain (text, ref index, endIndex, sentinels, throwOnError, out var domain))
				return false;

			// Note: The domain is deliberately not IDN-decoded so that the address is preserved exactly as it was written.
			// Use MailboxAddress.GetAddress (false) or MailboxAddress.GetDomain (false) to get the Unicode form.
			addrspec = localpart + "@" + domain;
			at = localpart.Length;

			return true;
		}

		Encoding GetEncoding (int codepage)
		{
			if (cachedEncoding != null && cachedCodepage == codepage)
				return cachedEncoding;

			if (!CharsetUtils.TryGetEncoding (codepage, out var encoding))
				return Encoding.UTF8;

			cachedCodepage = codepage;
			cachedEncoding = encoding;
			return encoding;
		}

		bool TryParseAngleMailbox (in AddressPhrase phrase, bool throwOnError, [NotNullWhen (true)] out InternetAddress? address)
		{
			string name = DecodePhraseName (phrase, out int codepage);
			var encoding = GetEncoding (codepage);
			int startIndex = phrase.StartIndex;
			DomainList? route = null;

			address = null;

			// skip over the '<'
			index++;

			// Note: check for excessive angle brackets like the example described in section 7.1.2 of rfc7103...
			if (index < endIndex && text[index] == (byte) '<') {
				if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Excessive angle brackets at offset {0}", text.GetByteOffset (index)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

					return false;
				}

				do {
					index++;
				} while (index < endIndex && text[index] == '<');
			}

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete mailbox at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

				return false;
			}

			if (text[index] == (byte) '@') {
				// Note: we always pass 'false' as the throwOnError argument here so that we can throw a more informative exception on error
				if (!DomainList.TryParse (text, ref index, endIndex, false, out route)) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid route in mailbox at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

					return false;
				}

				if (index >= endIndex || text[index] != (byte) ':') {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete route in mailbox at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

					return false;
				}

				// skip over ':'
				index++;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete mailbox at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

					return false;
				}
			}

			// Note: The only syntactically correct sentinel token here is the '>', but alas... to deal with the first example
			// in section 7.1.5 of rfc7103, we need to at least handle ',' as a sentinel and might as well handle ';' as well
			// in case the mailbox is within a group address.
			//
			// Example: <third@example.net, fourth@example.net>
			if (!TryParseAddrspec (CommaGreaterThanOrSemiColon, options.AddressParserComplianceMode, throwOnError, out string? addrspec, out int at))
				return false;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex || text[index] != (byte) '>') {
				if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected end of mailbox at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

					return false;
				}
			} else {
				// skip over the '>'
				index++;

				// Note: check for excessive angle brackets like the example described in section 7.1.2 of rfc7103...
				if (index < endIndex && text[index] == (byte) '>') {
					if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Excessive angle brackets at offset {0}", text.GetByteOffset (index)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

						return false;
					}

					do {
						index++;
					} while (index < endIndex && text[index] == '>');
				}
			}

			address = route != null
				? new MailboxAddress (encoding, name, route, addrspec, at)
				: new MailboxAddress (encoding, name, addrspec, at);

			return true;
		}

		bool TryParseGroup (AddressParserFlags flags, in AddressPhrase phrase, int groupDepth, [NotNullWhen (true)] out InternetAddress? address)
		{
			bool throwOnError = (flags & AddressParserFlags.ThrowOnError) != 0;
			int startIndex = phrase.StartIndex;

			address = null;

			if ((flags & AddressParserFlags.AllowGroupAddress) == 0) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Group address token at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

				return false;
			}

			if (groupDepth >= options.MaxAddressGroupDepth) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Exceeded maximum rfc822 group depth at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

				return false;
			}

			string name = DecodePhraseName (phrase, out int codepage);
			var encoding = GetEncoding (codepage);

			// skip over the ':'
			index++;

			while (index < endIndex && (text[index] == ':' || text.IsBlank (index)))
				index++;

			address = TryParseList (flags | AddressParserFlags.AllowMailboxAddress, true, groupDepth + 1, out var members)
				? new GroupAddress (encoding, name, members)
				: new GroupAddress (encoding, name);

			if (index >= endIndex || text[index] != (byte) ';') {
				if (throwOnError && options.AddressParserComplianceMode == RfcComplianceMode.Strict)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Expected to find ';' at offset {0}", text.GetByteOffset (index)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

				while (index < endIndex && text[index] != (byte) ';')
					index++;
			} else {
				index++;
			}

			return true;
		}

		// A provisional phrase range; its delimiter determines whether it is a name or an addr-spec.
		readonly struct AddressPhrase
		{
			public readonly int StartIndex;
			public readonly int Length;
			public readonly bool TrimLeadingQuote;

			public AddressPhrase (int startIndex, int length, bool trimLeadingQuote)
			{
				StartIndex = startIndex;
				Length = length;
				TrimLeadingQuote = trimLeadingQuote;
			}
		}

		bool TryScanPhrase (bool throwOnError, out AddressPhrase phrase)
		{
			phrase = default;

			// keep track of the start & length of the phrase
			bool trimLeadingQuote = false;
			int startIndex = index;
			int length = 0;

			while (index < endIndex) {
				bool quoted = text[index] == (byte) '"';

				if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
					if (!ParseUtils.SkipWord (text, ref index, endIndex, throwOnError))
						break;
				} else if (text[index] == (byte) '"') {
					int qstringIndex = index;

					if (!ParseUtils.SkipQuoted (text, ref index, endIndex, false)) {
						index = qstringIndex + 1;

						ParseUtils.SkipWhiteSpace (text, ref index, endIndex);

						if (!ParseUtils.SkipPhraseAtom (text, ref index, endIndex)) {
							if (throwOnError)
								throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete quoted-string token at offset {0}", text.GetByteOffset (qstringIndex)), text.GetByteOffset (qstringIndex), text.GetByteOffset (endIndex));

							break;
						}

						if (startIndex == qstringIndex)
							trimLeadingQuote = true;
					}
				} else {
					if (!ParseUtils.SkipPhraseAtom (text, ref index, endIndex))
						break;
				}

				length = index - startIndex;

				do {
					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;

					// Note: some clients don't quote dots in the name
					if (index >= endIndex || text[index] != (byte) '.')
						break;

					index++;

					length = index - startIndex;
				} while (true);

				// Note: some clients don't quote commas in the name
				if (options.AllowUnquotedCommasInAddresses && index < endIndex && text[index] == ',' && !quoted) {
					index++;

					length = index - startIndex;

					if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
						return false;
				}
			}

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			phrase = new AddressPhrase (startIndex, length, trimLeadingQuote);
			return true;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		string DecodePhraseName (in AddressPhrase phrase, out int codepage)
		{
			int nameIndex = phrase.StartIndex;
			int length = phrase.Length;

			codepage = -1;

			if (phrase.TrimLeadingQuote) {
				nameIndex++;
				length--;
			}

			// Unquote before decoding to preserve quotes and backslashes inside encoded-words.
			string name = length > 0 ? text.DecodePhrase (options, nameIndex, length, out codepage) : string.Empty;

			if (codepage == -1)
				codepage = 65001;

			return name;
		}

		bool TryParseLocalMailbox (AddressParserFlags flags, int startIndex, bool throwOnError, [NotNullWhen (true)] out InternetAddress? address)
		{
			address = null;

			// we've completely gobbled up an addr-spec w/o a domain
			char sentinel = index < endIndex ? text[index] : ',';
			string name;

			if ((flags & AddressParserFlags.AllowMailboxAddress) == 0) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Addr-spec token at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

				return false;
			}

			if (!options.AllowAddressesWithoutDomain) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete addr-spec token at offset {0}", text.GetByteOffset (startIndex)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

				return false;
			}

			// rewind back to the beginning of the local-part
			index = startIndex;

			if (!TryParseLocalPart (options.AddressParserComplianceMode, false, throwOnError, out var addrspec))
				return false;

			ParseUtils.SkipWhiteSpace (text, ref index, endIndex);

			if (index < endIndex && text[index] == '(') {
				int comment = index + 1;

				// Note: this can't fail because it has already been skipped in TryParseLocalPart() above.
				ParseUtils.SkipComment (text, ref index, endIndex);

				name = DecodeCommentName (comment, (index - 1) - comment);

				ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError);
			} else {
				name = string.Empty;
			}

			if (index < endIndex && text[index] == (byte) '>') {
				if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected '>' token at offset {0}", text.GetByteOffset (index)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

					return false;
				}

				index++;
			}

			if (index < endIndex && text[index] != sentinel) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected '{0}' token at offset {1}", text.GetErrorCharacter (index), text.GetByteOffset (index)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

				return false;
			}

			address = new MailboxAddress (Encoding.UTF8, name, addrspec, -1);

			return true;
		}

		bool TryParseBareMailbox (in AddressPhrase phrase, bool throwOnError, [NotNullWhen (true)] out InternetAddress? address)
		{
			int startIndex = phrase.StartIndex;

			address = null;

			string name;

			// rewind back to the beginning of the local-part
			index = startIndex;

			if (!TryParseAddrspec (CommaGreaterThanOrSemiColon, options.AddressParserComplianceMode, throwOnError, out var addrspec, out int at))
				return false;

			ParseUtils.SkipWhiteSpace (text, ref index, endIndex);

			if (index < endIndex && text[index] == '(') {
				int comment = index;

				if (!ParseUtils.SkipComment (text, ref index, endIndex)) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete comment token at offset {0}", text.GetByteOffset (comment)), text.GetByteOffset (comment), text.GetByteOffset (index));

					return false;
				}

				comment++;

				name = DecodeCommentName (comment, (index - 1) - comment);
			} else {
				name = string.Empty;
			}

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex) {
				address = new MailboxAddress (Encoding.UTF8, name, addrspec, at);
				return true;
			}

			if (text[index] == (byte) '<') {
				// We have an address like "user@example.com <user@example.com>"; i.e. the name is an unquoted string with an '@'.
				if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected '<' token at offset {0}", text.GetByteOffset (index)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

					return false;
				}

				int nameEndIndex = index;
				while (nameEndIndex > startIndex && text.IsWhitespace (nameEndIndex - 1))
					nameEndIndex--;

				var namePhrase = new AddressPhrase (startIndex, nameEndIndex - startIndex, phrase.TrimLeadingQuote);

				return TryParseAngleMailbox (namePhrase, throwOnError, out address);
			} else {
				// Note: since there was no '<', there should not be a '>'... but we handle it anyway in order to
				// deal with the second Unbalanced Angle Brackets example in section 7.1.3: second@example.org>
				if (text[index] == (byte) '>') {
					if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected '>' token at offset {0}", text.GetByteOffset (index)), text.GetByteOffset (startIndex), text.GetByteOffset (index));

						return false;
					}

					index++;
				}

				address = new MailboxAddress (Encoding.UTF8, name, addrspec, at);

				return true;
			}
		}

		internal bool TryParseAddress (AddressParserFlags flags, int groupDepth, [NotNullWhen (true)] out InternetAddress? address)
		{
			bool throwOnError = (flags & AddressParserFlags.ThrowOnError) != 0;

			address = null;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index == endIndex) {
				if (throwOnError)
					throw new ParseException ("No address found.", text.GetByteOffset (index), text.GetByteOffset (index));

				return false;
			}

			if (!TryScanPhrase (throwOnError, out var phrase))
				return false;

			if (index >= endIndex || text[index] == (byte) ',' || text[index] == (byte) '>' || text[index] == ';')
				return TryParseLocalMailbox (flags, phrase.StartIndex, throwOnError, out address);

			if (text[index] == (byte) ':')
				return TryParseGroup (flags, phrase, groupDepth, out address);

			if ((flags & AddressParserFlags.AllowMailboxAddress) == 0) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Mailbox address token at offset {0}", text.GetByteOffset (phrase.StartIndex)), text.GetByteOffset (phrase.StartIndex), text.GetByteOffset (index));

				return false;
			}

			if (text[index] == (byte) '@')
				return TryParseBareMailbox (phrase, throwOnError, out address);

			if (text[index] == (byte) '<')
				return TryParseAngleMailbox (phrase, throwOnError, out address);

			if (throwOnError)
				throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid address token at offset {0}", text.GetByteOffset (phrase.StartIndex)), text.GetByteOffset (phrase.StartIndex), text.GetByteOffset (index));

			return false;
		}

		// Resynchronize without interpreting quoted strings or comments as address separators.
		// Leave commas and the group terminator for the list and group productions.
		void SkipInvalidAddress (int startIndex, bool isGroup)
		{
			index = startIndex;

			while (index < endIndex) {
				if (text[index] == (byte) '"') {
					if (!ParseUtils.SkipQuoted (text, ref index, endIndex, false))
						break;
				} else if (text[index] == (byte) '(') {
					if (!ParseUtils.SkipComment (text, ref index, endIndex))
						break;
				} else if (text[index] == (byte) ',' || (isGroup && text[index] == (byte) ';')) {
					break;
				} else {
					index++;
				}
			}
		}

		bool TrySkipListSeparator (bool isGroup, bool throwOnError)
		{
			// Note: we loop here in case there are any extraneous commas
			bool skippedComma = false;

			do {
				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex)
					break;

				if (isGroup && text[index] == (byte) ';')
					break;

				if (text[index] != (byte) ',') {
					if (skippedComma)
						break;

					if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
						if (throwOnError) {
							if (isGroup)
								throw new ParseException ("Expected ',' between addresses or ';' to denote the end of a group of addresses.", text.GetByteOffset (index), text.GetByteOffset (index));
							else
								throw new ParseException ("Expected ',' between addresses.", text.GetByteOffset (index), text.GetByteOffset (index));
						}

						return false;
					} else {
						// start of a new address?
						break;
					}
				}

				skippedComma = true;
				index++;
			} while (true);

			return true;
		}

		internal bool TryParseList (AddressParserFlags flags, bool isGroup, int groupDepth, [NotNullWhen (true)] out List<InternetAddress>? addresses)
		{
			bool throwOnError = (flags & AddressParserFlags.ThrowOnError) != 0;
			var list = new List<InternetAddress> ();

			addresses = null;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index == endIndex) {
				if (throwOnError)
					throw new ParseException ("No addresses found.", text.GetByteOffset (index), text.GetByteOffset (index));

				return false;
			}

			while (index < endIndex) {
				if (isGroup && text[index] == (byte) ';')
					break;

				// Save state in case TryParseAddress() fails and we need to recover.
				int savedIndex = index;

				if (!TryParseAddress (flags, groupDepth, out var address)) {
					if ((flags & AddressParserFlags.Internal) == 0) {
						// Note: If flags contains the ThrowOnError flag, then TryParseAddress() would have thrown.
						return false;
					}

					SkipInvalidAddress (savedIndex, isGroup);
				} else {
					list.Add (address);
				}

				if (!TrySkipListSeparator (isGroup, throwOnError))
					return false;
			}

			addresses = list;

			return true;
		}
	}
}
