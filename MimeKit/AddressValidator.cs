//
// AddressValidator.cs
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

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// Validates the value of an address header against the address syntax defined by rfc5322.
	/// </summary>
	/// <remarks>
	/// <para>This validator is deliberately separate from <see cref="InternetAddress"/> and
	/// <see cref="InternetAddressList"/>, in the same way that the encoding validators are separate from
	/// the decoders: the parsers exist to recover a usable address from whatever a sender produced,
	/// while this class exists to report how far the input departed from the specification. Keeping
	/// them apart means that reporting a violation can never change what a message parses to.</para>
	/// <para>Because of that split, this class implements the <i>strict</i> grammar from rfc5322 and
	/// rfc6532 rather than attempting to predict what <see cref="InternetAddress"/> would do with the
	/// same input. It answers "does this conform?", not "what did MimeKit make of it?".</para>
	/// </remarks>
	class AddressValidator
	{
		readonly IMimeComplianceLogger logger;
		readonly long streamOffset;
		readonly int lineNumber;
		byte[] text;
		int startIndex;
		int endIndex;
		int index;

		/// <summary>
		/// Initialize a new instance of the <see cref="AddressValidator"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new address validator.
		/// </remarks>
		/// <param name="logger">The compliance logger.</param>
		/// <param name="streamOffset">The stream offset of the start of the value being validated.</param>
		/// <param name="lineNumber">The line number of the start of the value being validated.</param>
		public AddressValidator (IMimeComplianceLogger logger, long streamOffset, int lineNumber)
		{
			this.logger = logger;
			this.streamOffset = streamOffset;
			this.lineNumber = lineNumber;
			text = Array.Empty<byte> ();
		}

		void Log (MimeComplianceViolation violation, int at)
		{
			// Note: Header values are short and violations are rare, so the line number is counted on
			// demand rather than tracked on every advance, which would be easy to get subtly wrong.
			int line = lineNumber;

			for (int i = startIndex; i < at && i < endIndex; i++) {
				if (text[i] == (byte) '\n')
					line++;
			}

			logger.Log (new MimeComplianceIssue (violation, streamOffset + (at - startIndex), line));
		}

		bool SkipWhiteSpace ()
		{
			int start = index;

			while (index < endIndex && text[index].IsWhitespace ())
				index++;

			return index > start;
		}

		/// <summary>
		/// Skip over a comment, returning <see langword="false"/> if it was never closed.
		/// </summary>
		bool SkipComment ()
		{
			int start = index;
			int depth = 1;

			// skip over the opening '('
			index++;

			while (index < endIndex) {
				if (text[index] == (byte) '\\') {
					// quoted-pair
					index++;
					if (index >= endIndex)
						break;
				} else if (text[index] == (byte) '(') {
					depth++;
				} else if (text[index] == (byte) ')') {
					if (--depth == 0) {
						index++;
						return true;
					}
				}

				index++;
			}

			Log (MimeComplianceViolation.UnbalancedParenthesesInAddress, start);

			return false;
		}

		/// <summary>
		/// Skip over folding whitespace and comments, returning <see langword="false"/> if a comment
		/// was never closed (in which case the remainder of the value has been consumed).
		/// </summary>
		bool SkipCFWS ()
		{
			do {
				SkipWhiteSpace ();

				if (index >= endIndex || text[index] != (byte) '(')
					return true;

				if (!SkipComment ())
					return false;
			} while (true);
		}

		/// <summary>
		/// Skip over a quoted-string, returning <see langword="false"/> if it was never closed.
		/// </summary>
		bool SkipQuoted ()
		{
			int start = index;

			// skip over the opening '"'
			index++;

			while (index < endIndex) {
				if (text[index] == (byte) '\\') {
					index++;
					if (index >= endIndex)
						break;
				} else if (text[index] == (byte) '"') {
					index++;
					return true;
				}

				index++;
			}

			Log (MimeComplianceViolation.UnbalancedQuotesInAddress, start);

			return false;
		}

		/// <summary>
		/// Report null bytes and other control characters anywhere within the value.
		/// </summary>
		/// <remarks>
		/// <para>These are scanned for up front rather than token by token because they are not legal
		/// anywhere in a header value, so there is no position in which one needs to be tolerated, and
		/// because a control character derails the grammar: scanning first means the finding survives
		/// even when the structural parse of the surrounding address goes on to fail.</para>
		/// <para>Each class is reported once, at its first occurrence. A linefeed is deliberately
		/// excluded: folding whitespace is legal between address tokens, so a line break can only be
		/// judged in context and is handled during parsing instead. A carriage return with no linefeed
		/// after it is not excluded, because it cannot be part of a fold in any context.</para>
		/// </remarks>
		void ScanForControlCharacters ()
		{
			bool reportedNull = false, reportedControl = false;

			for (int i = startIndex; i < endIndex; i++) {
				byte c = text[i];

				if (c == 0) {
					if (!reportedNull) {
						Log (MimeComplianceViolation.NullByteInAddress, i);
						reportedNull = true;
					}
				} else if (IsControlCharacter (i)) {
					if (!reportedControl) {
						Log (MimeComplianceViolation.ControlCharacterInAddress, i);
						reportedControl = true;
					}
				}

				if (reportedNull && reportedControl)
					break;
			}
		}

		/// <summary>
		/// Determine whether the byte at the given index is a control character that cannot appear in
		/// an address.
		/// </summary>
		/// <remarks>
		/// A tab is whitespace rather than a control character, and a linefeed may be part of a fold.
		/// A carriage return may only ever appear as the first half of the CRLF of a fold: both
		/// <c>FWS</c> and <c>obs-FWS</c> require a linefeed and at least one whitespace character after
		/// it, and <c>obs-NO-WS-CTL</c> excludes both CR and LF, so a carriage return with nothing
		/// following it is illegal under every tier of the grammar, inside a quoted-string or a comment
		/// as much as anywhere else.
		/// </remarks>
		bool IsControlCharacter (int i)
		{
			byte c = text[i];

			if (c == (byte) '\r')
				return i + 1 >= endIndex || text[i + 1] != (byte) '\n';

			return (c < 0x20 && c != (byte) '\t' && c != (byte) '\n') || c == 0x7f;
		}

		/// <summary>
		/// Report a line break within a range of whitespace that appeared inside a token.
		/// </summary>
		/// <returns><see langword="true"/> if the whitespace contained a line break.</returns>
		/// <param name="start">The start of the whitespace.</param>
		/// <param name="end">The end of the whitespace.</param>
		/// <param name="reported">Whether a line break has already been reported for this address.</param>
		bool CheckLineBreak (int start, int end, ref bool reported)
		{
			bool found = false;

			for (int i = start; i < end; i++) {
				if (text[i] != (byte) '\r' && text[i] != (byte) '\n')
					continue;

				found = true;

				// Note: A carriage return with no linefeed after it has already been reported by
				// ScanForControlCharacters, which owns it because it is illegal regardless of where
				// it appears. The token still has to resume across it, but the same byte must not be
				// described twice, so keep looking for a genuine line break instead.
				if (text[i] == (byte) '\r' && (i + 1 >= endIndex || text[i + 1] != (byte) '\n'))
					continue;

				if (!reported) {
					Log (MimeComplianceViolation.LineBreakInAddress, i);
					reported = true;
				}

				break;
			}

			return found;
		}

		/// <summary>
		/// Determine whether a byte may be consumed as part of an atom.
		/// </summary>
		/// <remarks>
		/// Control characters are included deliberately. <see cref="ScanForControlCharacters"/> has
		/// already reported them, so absorbing them into the token they appear in loses nothing, and it
		/// keeps a single bad byte from splitting a token and cascading into a series of structural
		/// violations that describe nothing the sender actually did.
		/// </remarks>
		static bool IsAtomOrControl (byte c)
		{
			return c.IsAtom () || c == 0 || c == 0x7f || (c < 0x20 && c != (byte) '\t' && c != (byte) '\r' && c != (byte) '\n');
		}

		bool SkipAtom ()
		{
			int start = index;

			while (index < endIndex && IsAtomOrControl (text[index]))
				index++;

			return index > start;
		}

		/// <summary>
		/// Skip over a bracketed domain-literal without reporting anything.
		/// </summary>
		/// <remarks>
		/// A domain-literal is a single lexical token, so the specials inside it - most obviously the
		/// colons of an IPv6 literal - must not be mistaken for address-list delimiters.
		/// </remarks>
		void SkipDomainLiteral ()
		{
			// skip over the opening '['
			index++;

			while (index < endIndex && text[index] != (byte) ']')
				index++;

			if (index < endIndex)
				index++;
		}

		/// <summary>
		/// Validate a dot-atom, reporting the given violation if it contains an empty element (which
		/// covers consecutive dots as well as a leading or trailing dot).
		/// </summary>
		void ValidateDotAtom (MimeComplianceViolation violation)
		{
			do {
				int start = index;

				if (!SkipAtom ()) {
					// An empty element: either two dots in a row, or a dot at one end of the token.
					Log (violation, start);

					if (index >= endIndex || text[index] != (byte) '.')
						return;
				}

				if (index >= endIndex || text[index] != (byte) '.')
					return;

				// skip over the '.'
				index++;
			} while (true);
		}

		void ValidateDomainLiteral ()
		{
			int start = index;

			// skip over the opening '['
			index++;

			bool reportedWhiteSpace = false;

			while (index < endIndex && text[index] != (byte) ']') {
				// Note: A lone carriage return is whitespace, but ScanForControlCharacters has already
				// reported it as a control character, which is the more accurate description of it.
				if (text[index].IsWhitespace () && !reportedWhiteSpace && !IsControlCharacter (index)) {
					Log (MimeComplianceViolation.WhitespaceInDomainLiteral, index);
					reportedWhiteSpace = true;
				}

				index++;
			}

			if (index >= endIndex) {
				Log (MimeComplianceViolation.NonConformantAddress, start);
				return;
			}

			// skip over the closing ']'
			index++;
		}

		void ValidateDomain ()
		{
			if (index < endIndex && text[index] == (byte) '[') {
				ValidateDomainLiteral ();
				return;
			}

			int start = index;
			bool reportedLineBreak = false;

			do {
				if (!SkipAtom ()) {
					Log (MimeComplianceViolation.NonConformantAddress, start);
					return;
				}

				// Note: A conforming domain is a single dot-atom, which admits no comments or folding
				// whitespace between its atoms and dots. Anything else is obs-domain.
				int beforeCFWS = index;

				if (!SkipCFWS ())
					return;

				bool obsolete = index > beforeCFWS;

				if (index >= endIndex || text[index] != (byte) '.') {
					if (obsolete) {
						// Note: The token continues without an intervening '.', so this whitespace is
						// inside the dot-atom-text rather than around it.
						if (index < endIndex && IsAtomOrControl (text[index]) && CheckLineBreak (beforeCFWS, index, ref reportedLineBreak))
							continue;

						index = beforeCFWS;
					}

					return;
				}

				// skip over the '.'
				index++;

				beforeCFWS = index;

				if (!SkipCFWS ())
					return;

				if (index > beforeCFWS)
					obsolete = true;

				if (obsolete)
					Log (MimeComplianceViolation.ObsoleteDomainSyntax, beforeCFWS);

				if (index >= endIndex || !IsAtomOrControl (text[index])) {
					Log (MimeComplianceViolation.TrailingDotInDomain, index);
					return;
				}
			} while (true);
		}

		/// <summary>
		/// Validate an addr-spec, optionally allowing it to be terminated by '&gt;'.
		/// </summary>
		/// <param name="inAngleAddr">Whether the addr-spec is enclosed in angle brackets.</param>
		/// <param name="confirmedAddrSpec">
		/// Whether this is known to be an addr-spec rather than a phrase that is being re-read as one.
		/// Folding is legal between the words of a phrase but not inside the tokens of an addr-spec, so
		/// line breaks may only be reported when the distinction has already been made.
		/// </param>
		void ValidateAddrspec (bool inAngleAddr, bool confirmedAddrSpec)
		{
			int start = index;
			bool reportedLineBreak = false;

			do {
				// local-part = dot-atom / quoted-string
				if (index < endIndex && text[index] == (byte) '"') {
					if (!SkipQuoted ())
						return;
				} else {
					ValidateDotAtom (MimeComplianceViolation.InvalidLocalPart);
				}

				int beforeCFWS = index;

				if (!SkipCFWS ())
					return;

				// Note: rfc5322 permits CFWS around a dot-atom but none inside its dot-atom-text, so
				// whitespace followed by more of the same token is not legal folding.
				if (!confirmedAddrSpec || index == beforeCFWS || index >= endIndex || !IsAtomOrControl (text[index]))
					break;

				// Once the line break has been reported, carry on as though the whitespace were not
				// there, so that the rest of the addr-spec is described accurately rather than
				// cascading into structural violations the sender is not responsible for. Ordinary
				// embedded whitespace is left to the existing handling below.
				if (!CheckLineBreak (beforeCFWS, index, ref reportedLineBreak))
					break;
			} while (true);

			if (index >= endIndex || text[index] != (byte) '@') {
				// Note: An addr-spec with no domain. Inside an angle-addr this is still a naked
				// local-part; outside of one it is section 7.1.7 of rfc7103.
				if (index >= endIndex || text[index] == (byte) ',' || text[index] == (byte) ';' || (inAngleAddr && text[index] == (byte) '>'))
					Log (MimeComplianceViolation.AddressWithoutDomain, start);
				else
					Log (MimeComplianceViolation.NonConformantAddress, index);

				return;
			}

			// skip over the '@'
			index++;

			if (!SkipCFWS ())
				return;

			if (index >= endIndex) {
				Log (MimeComplianceViolation.NonConformantAddress, start);
				return;
			}

			ValidateDomain ();
		}

		void ValidateAngleAddr ()
		{
			int start = index;

			// skip over the '<'
			index++;

			if (!SkipCFWS ())
				return;

			// Note: Section 7.1.2 of rfc7103.
			if (index < endIndex && text[index] == (byte) '<') {
				Log (MimeComplianceViolation.ExcessiveAngleBracketsInAddress, index);

				while (index < endIndex && text[index] == (byte) '<')
					index++;

				if (!SkipCFWS ())
					return;
			}

			// Note: The obs-route production from section 4.4 of rfc5322.
			if (index < endIndex && text[index] == (byte) '@') {
				Log (MimeComplianceViolation.ObsoleteRouteAddress, index);

				while (index < endIndex && text[index] != (byte) ':' && text[index] != (byte) '>')
					index++;

				if (index < endIndex && text[index] == (byte) ':')
					index++;

				if (!SkipCFWS ())
					return;
			}

			ValidateAddrspec (true, true);

			if (!SkipCFWS ())
				return;

			if (index >= endIndex || text[index] != (byte) '>') {
				Log (MimeComplianceViolation.UnbalancedAngleBracketsInAddress, start);
				return;
			}

			// skip over the '>'
			index++;

			// Note: Section 7.1.2 of rfc7103.
			if (index < endIndex && text[index] == (byte) '>') {
				Log (MimeComplianceViolation.ExcessiveAngleBracketsInAddress, index);

				while (index < endIndex && text[index] == (byte) '>')
					index++;
			}
		}

		/// <summary>
		/// Scan a phrase, recording whether it contained a character that would have had to be quoted.
		/// </summary>
		/// <remarks>
		/// The same leading token can turn out to be a display-name or an addr-spec depending on what
		/// follows it, so the decision about whether an unquoted special is a violation cannot be made
		/// until the delimiter is known.
		/// </remarks>
		/// <param name="unquotedSpecial">Whether the phrase contained a character that would have had to be quoted.</param>
		/// <param name="specialIndex">The index of the first such character.</param>
		/// <param name="hasContent">Whether the phrase contained anything at all besides comments and whitespace.</param>
		bool ScanPhrase (out bool unquotedSpecial, out int specialIndex, out bool hasContent)
		{
			unquotedSpecial = false;
			specialIndex = -1;
			hasContent = false;

			do {
				if (!SkipCFWS ())
					return false;

				if (index >= endIndex)
					return true;

				byte c = text[index];

				if (c == (byte) '"') {
					hasContent = true;

					if (!SkipQuoted ())
						return false;
				} else if (c == (byte) '<' || c == (byte) ':' || c == (byte) ',' || c == (byte) ';' || c == (byte) '>') {
					return true;
				} else if (c == (byte) '@' || c == (byte) '.') {
					hasContent = true;

					if (!unquotedSpecial) {
						unquotedSpecial = true;
						specialIndex = index;
					}

					index++;
				} else if (c == (byte) '[') {
					hasContent = true;

					if (!unquotedSpecial) {
						unquotedSpecial = true;
						specialIndex = index;
					}

					SkipDomainLiteral ();
				} else if (IsAtomOrControl (c)) {
					hasContent = true;

					SkipAtom ();
				} else {
					// Some other character that cannot appear in a phrase at all.
					hasContent = true;

					if (!unquotedSpecial) {
						unquotedSpecial = true;
						specialIndex = index;
					}

					index++;
				}
			} while (true);
		}

		void ValidateGroup (bool unquotedSpecial, int specialIndex)
		{
			if (unquotedSpecial)
				Log (MimeComplianceViolation.UnquotedDisplayName, specialIndex);

			int start = index;

			// skip over the ':'
			index++;

			do {
				if (!SkipCFWS ())
					return;

				if (index >= endIndex)
					break;

				if (text[index] == (byte) ';') {
					// skip over the ';'
					index++;
					return;
				}

				if (text[index] == (byte) ',') {
					Log (MimeComplianceViolation.ExtraneousCommaInAddressList, index);
					index++;
					continue;
				}

				int before = index;

				if (!ValidateAddress (true))
					return;

				if (index == before) {
					// Defensive: never spin on a character no production consumed.
					Log (MimeComplianceViolation.NonConformantAddress, index);
					index++;
					continue;
				}

				if (!SkipCFWS ())
					return;

				if (index >= endIndex)
					break;

				if (text[index] == (byte) ';') {
					index++;
					return;
				}

				if (text[index] != (byte) ',') {
					Log (MimeComplianceViolation.MissingAddressSeparator, index);
					continue;
				}

				// skip over the ','
				index++;
			} while (true);

			Log (MimeComplianceViolation.MissingGroupTerminator, start);
		}

		/// <summary>
		/// Validate a single address, returning <see langword="false"/> if the remainder of the value
		/// has been consumed and scanning cannot usefully continue.
		/// </summary>
		bool ValidateAddress (bool inGroup)
		{
			int start = index;

			if (!ScanPhrase (out bool unquotedSpecial, out int specialIndex, out bool hasContent))
				return false;

			if (index >= endIndex) {
				// The whole token was an addr-spec rather than a display-name.
				bool confirmed = ContainsAt (start, index);

				index = start;
				ValidateAddrspec (false, confirmed);
				return true;
			}

			byte c = text[index];

			if (c == (byte) ':') {
				if (inGroup) {
					// rfc5322 does not permit a group inside a group.
					Log (MimeComplianceViolation.NonConformantAddress, index);
					index++;
					return true;
				}

				if (!hasContent) {
					// A display-name is a phrase, which requires at least one word.
					Log (MimeComplianceViolation.EmptyGroupName, start);
				}

				ValidateGroup (unquotedSpecial, specialIndex);
				return true;
			}

			if (c == (byte) '<') {
				if (unquotedSpecial)
					Log (MimeComplianceViolation.UnquotedDisplayName, specialIndex);

				ValidateAngleAddr ();
				return true;
			}

			if (c == (byte) '>') {
				// A closing bracket with nothing to close: section 7.1.3 of rfc7103.
				Log (MimeComplianceViolation.UnbalancedAngleBracketsInAddress, index);
				index++;
				return true;
			}

			// ',' or ';': the phrase we just scanned was really an addr-spec.
			bool hasAt = ContainsAt (start, index);

			if (c == (byte) ',' && !hasAt && FollowedByNameAddr (index + 1)) {
				// Section 7.1.5 of rfc7103: an unquoted comma inside a display-name splits one
				// address into what looks like a naked local-part followed by a name-addr. Report
				// it as the display-name defect it is rather than as a domainless address, and let
				// the remainder of the display-name be scanned as the next address. The comma is
				// deliberately left for the caller to consume as the list separator.
				Log (MimeComplianceViolation.UnquotedDisplayName, start);

				return true;
			}

			index = start;
			ValidateAddrspec (false, hasAt);

			return true;
		}

		/// <summary>
		/// Determine whether the given range contains an unquoted '@'.
		/// </summary>
		bool ContainsAt (int start, int end)
		{
			for (int i = start; i < end; i++) {
				if (text[i] == (byte) '"') {
					// Skip over the quoted-string; an '@' inside it is part of the local-part.
					for (i++; i < end; i++) {
						if (text[i] == (byte) '\\')
							i++;
						else if (text[i] == (byte) '"')
							break;
					}
				} else if (text[i] == (byte) '@') {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determine whether the next address in the list is a name-addr.
		/// </summary>
		/// <remarks>
		/// This is the one place the validator looks ahead. It exists solely to tell an unquoted
		/// comma in a display-name apart from a genuine list of domainless addresses, which are
		/// otherwise the same token sequence.
		/// </remarks>
		bool FollowedByNameAddr (int start)
		{
			for (int i = start; i < endIndex; i++) {
				byte c = text[i];

				if (c == (byte) '"') {
					for (i++; i < endIndex; i++) {
						if (text[i] == (byte) '\\')
							i++;
						else if (text[i] == (byte) '"')
							break;
					}
				} else if (c == (byte) '<') {
					return true;
				} else if (c == (byte) ',' || c == (byte) ';' || c == (byte) ':') {
					// The end of the next address without an angle-addr in it.
					return false;
				}
			}

			return false;
		}

		/// <summary>
		/// Validate the value of an address header.
		/// </summary>
		/// <remarks>
		/// Validates the value of an address header, reporting any deviations from rfc5322 to the
		/// compliance logger that the validator was created with.
		/// </remarks>
		/// <param name="buffer">The raw header value.</param>
		/// <param name="startIndex">The starting index of the value within the buffer.</param>
		/// <param name="length">The length of the value.</param>
		public void Validate (byte[] buffer, int startIndex, int length)
		{
			text = buffer;
			this.startIndex = startIndex;
			endIndex = startIndex + length;
			index = startIndex;

			// Note: The raw value still carries the line terminator that ended the header (and any
			// trailing folding whitespace). That is not part of the address list, and leaving it in
			// makes a trailing token look as though it were followed by folding whitespace.
			while (endIndex > startIndex && text[endIndex - 1].IsWhitespace ())
				endIndex--;

			if (endIndex == startIndex)
				return;

			ScanForControlCharacters ();

			// Note: rfc6532 extends the address grammar to UTF-8 and to nothing else, so any 8-bit
			// content that is not valid UTF-8 has no defined interpretation.
			if (!Utf8.IsValid (new ReadOnlySpan<byte> (buffer, startIndex, length)))
				Log (MimeComplianceViolation.Invalid8BitAddress, startIndex);

			bool any = false;

			do {
				if (!SkipCFWS ())
					return;

				if (index >= endIndex)
					break;

				if (text[index] == (byte) ',') {
					// A comma with no address in front of it.
					Log (MimeComplianceViolation.ExtraneousCommaInAddressList, index);
					index++;
					continue;
				}

				if (text[index] == (byte) ';') {
					// A group terminator with no group to terminate.
					Log (MimeComplianceViolation.NonConformantAddress, index);
					index++;
					continue;
				}

				int before = index;

				if (!ValidateAddress (false))
					return;

				any = true;

				if (index == before) {
					// Defensive: never spin on a character no production consumed.
					Log (MimeComplianceViolation.NonConformantAddress, index);
					index++;
					continue;
				}

				if (!SkipCFWS ())
					return;

				if (index >= endIndex)
					break;

				if (text[index] != (byte) ',') {
					Log (MimeComplianceViolation.MissingAddressSeparator, index);
					continue;
				}

				// skip over the ','
				index++;

				// A trailing comma is an empty final element.
				int afterComma = index;

				if (!SkipCFWS ())
					return;

				if (index >= endIndex)
					Log (MimeComplianceViolation.ExtraneousCommaInAddressList, afterComma);
			} while (true);

			if (!any)
				Log (MimeComplianceViolation.NonConformantAddress, startIndex);
		}
	}
}
