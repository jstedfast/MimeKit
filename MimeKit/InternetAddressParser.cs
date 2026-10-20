//
// InternetAddressParser.cs
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

using MimeKit.Utils;

namespace MimeKit {
	// Owns a complete input slice. Group and list parsing share the same cursor.
	internal struct InternetAddressParser
	{
		readonly ParserOptions options;
		readonly byte[] text;
		readonly int endIndex;
		int index;

		internal InternetAddressParser (ParserOptions options, byte[] text, int startIndex, int endIndex)
		{
			this.options = options;
			this.text = text;
			this.endIndex = endIndex;
			index = startIndex;
		}

		internal int Position => index;

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
				byte c = text[index];

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
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid control character in quoted-string token at offset {0}", startIndex), startIndex, index);

						return false;
					}
				} else {
					// Note: We are going to make the decision to not support any escaped ctrl characters even
					// though they are allowed by obs-qp. They are just too dangerous.
					if ((c < 32 && c != 9) || c == 127) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid quoted-pair in quoted-string token at offset {0}", startIndex), startIndex, index);

						return false;
					}

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
				} else if (text[index].IsAtom ()) {
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
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid local-part at offset {0}", startIndex), startIndex, index);

					return false;
				}

				string word;

				try {
					word = CharsetUtils.UTF8.GetString (text, start, index - start);
				} catch (DecoderFallbackException ex) {
					if (compliance == RfcComplianceMode.Strict) {
						if (throwOnError)
							throw new ParseException ("Internationalized local-part tokens may only contain UTF-8 characters.", start, start, ex);

						return false;
					}

					word = CharsetUtils.Latin1.GetString (text, start, index - start);
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
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete local-part at offset {0}", startIndex), startIndex, index);

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
			if (text.AsSpan (startIndex, length).IndexOf ((byte) '\t') == -1)
				return Rfc2047.DecodePhrase (options, text, startIndex, length).Trim ();

			var buffer = new byte[length];

			for (int i = 0; i < length; i++) {
				byte c = text[startIndex + i];

				buffer[i] = c == (byte) '\t' ? (byte) ' ' : c;
			}

			return Rfc2047.DecodePhrase (options, buffer, 0, length).Trim ();
		}

		static ReadOnlySpan<byte> CommaGreaterThanOrSemiColon => ",>;"u8;

		internal bool TryParseAddrspec (ReadOnlySpan<byte> sentinels, RfcComplianceMode compliance, bool throwOnError, [NotNullWhen (true)] out string? addrspec, out int at)
		{
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
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid addr-spec token at offset {0}", startIndex), startIndex, index);

				return false;
			}

			index++;
			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete addr-spec token at offset {0}", startIndex), startIndex, index);

				return false;
			}

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index >= endIndex) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete addr-spec token at offset {0}", startIndex), startIndex, index);

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

		bool TryParseMailbox (int startIndex, string name, int codepage, bool throwOnError, [NotNullWhen (true)] out InternetAddress? address)
		{
			var encoding = CharsetUtils.GetEncodingOrDefault (codepage, Encoding.UTF8);
			DomainList? route = null;

			address = null;

			// skip over the '<'
			index++;

			// Note: check for excessive angle brackets like the example described in section 7.1.2 of rfc7103...
			if (index < endIndex && text[index] == (byte) '<') {
				if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Excessive angle brackets at offset {0}", index), startIndex, index);

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
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete mailbox at offset {0}", startIndex), startIndex, index);

				return false;
			}

			if (text[index] == (byte) '@') {
				// Note: we always pass 'false' as the throwOnError argument here so that we can throw a more informative exception on error
				if (!DomainList.TryParse (text, ref index, endIndex, false, out route)) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid route in mailbox at offset {0}", startIndex), startIndex, index);

					return false;
				}

				if (index >= endIndex || text[index] != (byte) ':') {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete route in mailbox at offset {0}", startIndex), startIndex, index);

					return false;
				}

				// skip over ':'
				index++;

				if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
					return false;

				if (index >= endIndex) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete mailbox at offset {0}", startIndex), startIndex, index);

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
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected end of mailbox at offset {0}", startIndex), startIndex, index);

					return false;
				}
			} else {
				// skip over the '>'
				index++;

				// Note: check for excessive angle brackets like the example described in section 7.1.2 of rfc7103...
				if (index < endIndex && text[index] == (byte) '>') {
					if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
						if (throwOnError)
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Excessive angle brackets at offset {0}", index), startIndex, index);

						return false;
					}

					do {
						index++;
					} while (index < endIndex && text[index] == '>');
				}
			}

			if (route != null)
				address = new MailboxAddress (encoding, name, route, addrspec, at);
			else
				address = new MailboxAddress (encoding, name, addrspec, at);

			return true;
		}

		bool TryParseGroup (AddressParserFlags flags, int startIndex, int groupDepth, string name, int codepage, out InternetAddress address)
		{
			var encoding = CharsetUtils.GetEncodingOrDefault (codepage, Encoding.UTF8);
			bool throwOnError = (flags & AddressParserFlags.ThrowOnError) != 0;

			// skip over the ':'
			index++;

			while (index < endIndex && (text[index] == ':' || text[index].IsBlank ()))
				index++;

			if (TryParseList (flags | AddressParserFlags.AllowMailboxAddress, true, groupDepth, out var members))
				address = new GroupAddress (encoding, name, members);
			else
				address = new GroupAddress (encoding, name);

			if (index >= endIndex || text[index] != (byte) ';') {
				if (throwOnError && options.AddressParserComplianceMode == RfcComplianceMode.Strict)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Expected to find ';' at offset {0}", index), startIndex, index);

				while (index < endIndex && text[index] != (byte) ';')
					index++;
			} else {
				index++;
			}

			return true;
		}

		internal bool TryParseAddress (AddressParserFlags flags, int groupDepth, [NotNullWhen (true)] out InternetAddress? address)
		{
			bool throwOnError = (flags & AddressParserFlags.ThrowOnError) != 0;

			address = null;

			if (!ParseUtils.SkipCommentsAndWhiteSpace (text, ref index, endIndex, throwOnError))
				return false;

			if (index == endIndex) {
				if (throwOnError)
					throw new ParseException ("No address found.", index, index);

				return false;
			}

			// keep track of the start & length of the phrase
			bool trimLeadingQuote = false;
			int startIndex = index;
			int length = 0;
			int words = 0;

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
								throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete quoted-string token at offset {0}", qstringIndex), qstringIndex, endIndex);

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

				words++;

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

			// specials    =  "(" / ")" / "<" / ">" / "@"  ; Must be in quoted-
			//             /  "," / ";" / ":" / "\" / <">  ;  string, to use
			//             /  "." / "[" / "]"              ;  within a word.

			if (index >= endIndex || text[index] == (byte) ',' || text[index] == (byte) '>' || text[index] == ';') {
				// we've completely gobbled up an addr-spec w/o a domain
				byte sentinel = index < endIndex ? text[index] : (byte) ',';
				string name;

				if ((flags & AddressParserFlags.AllowMailboxAddress) == 0) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Addr-spec token at offset {0}", startIndex), startIndex, index);

					return false;
				}

				if (!options.AllowAddressesWithoutDomain) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete addr-spec token at offset {0}", startIndex), startIndex, index);

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
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected '>' token at offset {0}", index), startIndex, index);

						return false;
					}

					index++;
				}

				if (index < endIndex && text[index] != sentinel) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected '{0}' token at offset {1}", (char) text[index], index), startIndex, index);

					return false;
				}

				address = new MailboxAddress (Encoding.UTF8, name, addrspec, -1);

				return true;
			}

			if (text[index] == (byte) ':') {
				// rfc2822 group address
				int nameIndex = startIndex;
				int codepage = -1;
				string name;

				if ((flags & AddressParserFlags.AllowGroupAddress) == 0) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Group address token at offset {0}", startIndex), startIndex, index);

					return false;
				}

				if (groupDepth >= options.MaxAddressGroupDepth) {
					if (throwOnError)
						throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Exceeded maximum rfc822 group depth at offset {0}", startIndex), startIndex, index);

					return false;
				}

				if (trimLeadingQuote) {
					nameIndex++;
					length--;
				}

				// Note: unquote *before* decoding so that quotes and backslashes that were inside of encoded-words are preserved.
				if (length > 0) {
					var unquoted = MimeUtils.Unquote (text, nameIndex, length, true);

					name = Rfc2047.DecodePhrase (options, unquoted, 0, unquoted.Length, out codepage);
				} else {
					name = string.Empty;
				}

				if (codepage == -1)
					codepage = 65001;

				return TryParseGroup (flags, startIndex, groupDepth + 1, name, codepage, out address);
			}

			if ((flags & AddressParserFlags.AllowMailboxAddress) == 0) {
				if (throwOnError)
					throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Mailbox address token at offset {0}", startIndex), startIndex, index);

				return false;
			}

			if (text[index] == (byte) '@') {
				// we're either in the middle of an addr-spec token or we completely gobbled up an addr-spec w/o a domain
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
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Incomplete comment token at offset {0}", comment), comment, index);

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
							throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected '<' token at offset {0}", index), startIndex, index);

						return false;
					}

					int nameEndIndex = index;
					while (nameEndIndex > startIndex && text[nameEndIndex - 1].IsWhitespace ())
						nameEndIndex--;

					length = nameEndIndex - startIndex;

					// fall through to the rfc822 angle-addr token case...
				} else {
					// Note: since there was no '<', there should not be a '>'... but we handle it anyway in order to
					// deal with the second Unbalanced Angle Brackets example in section 7.1.3: second@example.org>
					if (text[index] == (byte) '>') {
						if (options.AddressParserComplianceMode == RfcComplianceMode.Strict) {
							if (throwOnError)
								throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Unexpected '>' token at offset {0}", index), startIndex, index);

							return false;
						}

						index++;
					}

					address = new MailboxAddress (Encoding.UTF8, name, addrspec, at);

					return true;
				}
			}

			if (text[index] == (byte) '<') {
				// rfc2822 angle-addr token
				int nameIndex = startIndex;
				int codepage = -1;
				string name;

				if (trimLeadingQuote) {
					nameIndex++;
					length--;
				}

				if (length > 0) {
					var unquoted = MimeUtils.Unquote (text, nameIndex, length, true);

					name = Rfc2047.DecodePhrase (options, unquoted, 0, unquoted.Length, out codepage);
				} else {
					name = string.Empty;
				}

				if (codepage == -1)
					codepage = 65001;

				return TryParseMailbox (startIndex, name, codepage, throwOnError, out address);
			}

			if (throwOnError)
				throw new ParseException (string.Format (CultureInfo.InvariantCulture, "Invalid address token at offset {0}", startIndex), startIndex, index);

			return false;
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
					throw new ParseException ("No addresses found.", index, index);

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

					// When the 'Internal' flag is set, it means we are being called from places like MimeMessage or
					// Header (when trying to reformat an existing header value) where we want to be more lenient with
					// parsing and simply skip any addresses that we fail to parse instead of throwing an exception or
					// prematurely abort parsing. In other words, we want to preserve as many valid addresses as possible.
					//
					// With that in mind, when TryParseAddress() fails, skip to the next address and resume parsing.
					index = savedIndex;

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
				} else {
					list.Add (address);
				}

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
									throw new ParseException ("Expected ',' between addresses or ';' to denote the end of a group of addresses.", index, index);
								else
									throw new ParseException ("Expected ',' between addresses.", index, index);
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
			}

			addresses = list;

			return true;
		}

	}
}
