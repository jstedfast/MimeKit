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
using System.Collections.Generic;

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
		readonly MimeComplianceContext context;
		readonly long streamOffset;
		readonly int lineNumber;
		readonly int columnNumber;
		byte[] text;
		int startIndex;
		int endIndex;
		int index;

		// Note: The positions of the ISO-2022 shift and escape sequences that ScanForControlCharacters
		// found. They are held back rather than reported where they are found because how they should
		// be described depends on where they turn out to be, which is not known until the value has
		// been parsed. This stays null for the overwhelming majority of values, which contain none.
		List<int>? iso2022;
		bool reportedControlCharacter;
		bool reportedStrayCarriageReturn;
		bool reportedIso2022LocalPart;

		/// <summary>
		/// Initialize a new instance of the <see cref="AddressValidator"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new address validator.
		/// </remarks>
		/// <param name="logger">The compliance logger.</param>
		/// <param name="context">The context that the message is being used in.</param>
		/// <param name="streamOffset">The stream offset of the start of the value being validated.</param>
		/// <param name="lineNumber">The line number of the start of the value being validated.</param>
		/// <param name="columnNumber">The one-based column number of the start of the value being validated.</param>
		public AddressValidator (IMimeComplianceLogger logger, MimeComplianceContext context, long streamOffset, int lineNumber, int columnNumber)
		{
			this.logger = logger;
			this.context = context;
			this.streamOffset = streamOffset;
			this.lineNumber = lineNumber;
			this.columnNumber = columnNumber;
			text = Array.Empty<byte> ();
		}

		void Log (MimeComplianceViolation violation, int at)
		{
			// Note: Header values are short and violations are rare, so the position is worked out on
			// demand rather than tracked on every advance, which would be easy to get subtly wrong.
			// The column falls out of the same scan that counts the lines.
			int line = lineNumber;
			int lineBegin = -1;

			for (int i = startIndex; i < at && i < endIndex; i++) {
				if (text[i] == (byte) '\n') {
					line++;
					lineBegin = i + 1;
				}
			}

			// Note: Until the value has been folded, the column is still relative to the field name
			// that preceded it on the same line.
			int column = lineBegin < 0 ? columnNumber + (at - startIndex) : (at - lineBegin) + 1;

			logger.Log (new MimeComplianceIssue (context, violation, streamOffset + (at - startIndex), line, column));
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
				} else if (text[index] == (byte) ')' && --depth == 0) {
					index++;
					return true;
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
		/// after it is not excluded, because it cannot be part of a fold in any context; it is
		/// reported as <see cref="MimeComplianceViolation.LineBreakInAddress"/> rather than as a
		/// control character, since what makes it dangerous is that it is half of a line break.</para>
		/// <para>ISO-2022 shift and escape sequences are recorded rather than reported. Inside a
		/// local-part they are a legacy Japanese mailer convention rather than an arbitrary control
		/// character, and telling the two apart needs the parse, so the decision is deferred to
		/// <see cref="CheckIso2022LocalPart"/> and <see cref="ReportUnattributedIso2022Sequences"/>.</para>
		/// </remarks>
		void ScanForControlCharacters ()
		{
			bool reportedNull = false;

			// Note: This no longer stops as soon as one of each class has been reported, because every
			// ISO-2022 sequence has to be recorded in order to be attributed later. Header values are
			// short enough that scanning the rest of one costs nothing worth saving.
			for (int i = startIndex; i < endIndex; i++) {
				byte c = text[i];

				if (c == 0) {
					if (!reportedNull) {
						Log (MimeComplianceViolation.NullByteInAddress, i);
						reportedNull = true;
					}
				} else if (IsIso2022Sequence (i, out int length)) {
					(iso2022 ??= new List<int> ()).Add (i);

					// Note: Step over the rest of the sequence so that its intermediate and final
					// bytes are not considered again.
					i += length - 1;
				} else if (IsControlCharacter (i)) {
					// Note: A carriage return reaching here is one with no linefeed after it, which is
					// the primitive a header injection is built from: a transport that normalizes it
					// to CRLF turns it into a header split. Describing it as a line break rather than
					// as a generic control character keeps it at the same severity as the folded form
					// in CheckLineBreak, which is the far more benign of the two and would otherwise
					// outrank it.
					if (text[i] == (byte) '\r') {
						if (!reportedStrayCarriageReturn) {
							Log (MimeComplianceViolation.LineBreakInAddress, i);
							reportedStrayCarriageReturn = true;
						}
					} else if (!reportedControlCharacter) {
						Log (MimeComplianceViolation.ControlCharacterInAddress, i);
						reportedControlCharacter = true;
					}
				}
			}
		}

		/// <summary>
		/// Determine whether an ISO-2022 shift or escape sequence begins at the given index, and if so,
		/// how many bytes long it is.
		/// </summary>
		/// <remarks>
		/// <para>An escape is only recognized as a character set designation when it is followed by at
		/// least one intermediate byte in the <c>0x20..0x2f</c> range and then a final byte in the
		/// <c>0x30..0x7e</c> range, which is the shape of every ISO-2022 designation sequence: the
		/// <c>ESC ( B</c> and <c>ESC $ B</c> of rfc1468, the <c>ESC $ ( D</c> and <c>ESC . A</c> added by
		/// rfc1554, and the <c>ESC $ ) C</c> of rfc1557 among them.</para>
		/// <para>Requiring an intermediate byte is what keeps an escape that merely happens to precede a
		/// letter from being mistaken for a designation. Such an escape is an ordinary control character
		/// and is reported as one.</para>
		/// <para>Shift-out and shift-in are recognized on their own. ISO-2022-JP proper has no use for
		/// them, but the Korean and Chinese variants use them to switch between the designated sets, and
		/// neither byte has any other meaning in a header.</para>
		/// </remarks>
		bool IsIso2022Sequence (int i, out int length)
		{
			byte c = text[i];

			if (c == 0x0e || c == 0x0f) {
				length = 1;
				return true;
			}

			length = 0;

			if (c != 0x1b)
				return false;

			int j = i + 1;

			while (j < endIndex && text[j] >= 0x20 && text[j] <= 0x2f)
				j++;

			if (j == i + 1 || j >= endIndex || text[j] < 0x30 || text[j] > 0x7e)
				return false;

			length = (j - i) + 1;

			return true;
		}

		/// <summary>
		/// Attribute any recorded ISO-2022 sequences that fall within the given range to the local-part
		/// that occupies it.
		/// </summary>
		/// <remarks>
		/// Sequences claimed here are struck from the pending set so that the same bytes are not also
		/// described as control characters by <see cref="ReportUnattributedIso2022Sequences"/>.
		/// </remarks>
		/// <param name="start">The start of the local-part.</param>
		/// <param name="end">The end of the local-part.</param>
		void CheckIso2022LocalPart (int start, int end)
		{
			if (iso2022 is null)
				return;

			for (int i = 0; i < iso2022.Count; i++) {
				int at = iso2022[i];

				if (at < start || at >= end)
					continue;

				iso2022[i] = -1;

				if (!reportedIso2022LocalPart) {
					Log (MimeComplianceViolation.Iso2022SequenceInLocalPart, at);
					reportedIso2022LocalPart = true;
				}
			}
		}

		/// <summary>
		/// Report the first ISO-2022 sequence that did not turn out to be inside a local-part as the
		/// control character that it is.
		/// </summary>
		/// <remarks>
		/// This has to run after the parse, because nothing before it can say where a sequence ended up,
		/// but it runs on every exit path so that the finding still survives a parse that gave up early.
		/// </remarks>
		void ReportUnattributedIso2022Sequences ()
		{
			if (iso2022 is null || reportedControlCharacter)
				return;

			for (int i = 0; i < iso2022.Count; i++) {
				if (iso2022[i] < 0)
					continue;

				Log (MimeComplianceViolation.ControlCharacterInAddress, iso2022[i]);
				reportedControlCharacter = true;
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
		/// Report a line break within a range that appeared inside a token.
		/// </summary>
		/// <returns><see langword="true"/> if the range contained a line break.</returns>
		/// <param name="start">The start of the range.</param>
		/// <param name="end">The end of the range.</param>
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
				// described twice, so keep looking for a genuine line break instead. Both paths now
				// report LineBreakInAddress, so this only avoids logging that violation twice.
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

		/// <summary>
		/// Determine whether a byte could begin an address, once the list separators and any
		/// preceding CFWS have been dealt with by the caller.
		/// </summary>
		/// <remarks>
		/// This is not a grammar production but a progress guarantee. An address is a phrase, an
		/// addr-spec or an angle-addr, so it begins with atom text, a quoted-string or a '&lt;'. The
		/// '.', '@' and ':' are not legal starts either, but each of them is consumed by the
		/// productions that report them, so they are admitted here to leave that reporting alone. Any
		/// other byte is one no production can consume, which makes re-entering the parser at it a
		/// guaranteed no-op.
		/// </remarks>
		static bool CanBeginAddress (byte c)
		{
			return IsAtomOrControl (c) || c == (byte) '"' || c == (byte) '<' || c == (byte) '.' || c == (byte) '@' || c == (byte) ':';
		}

		/// <summary>
		/// Determine whether a byte delimits the elements of an address list or a group.
		/// </summary>
		static bool IsAddressListSeparator (byte c)
		{
			return c == (byte) ',' || c == (byte) ';';
		}

		/// <summary>
		/// Advance to the next byte either address-list loop could make progress on.
		/// </summary>
		/// <remarks>
		/// Both loops report a missing separator and then re-enter the parser at the offending byte,
		/// on the assumption that a new address begins there. When the byte is one no production can
		/// consume, that assumption is wrong: the previous address stopped precisely because of it,
		/// and re-entering re-walks the same token, describing it a second time before the
		/// no-progress guard breaks the spin. Skipping the whole run of such bytes keeps each one
		/// described once and keeps a run from tripping that guard repeatedly. The run stops at a
		/// list separator, which the loops consume themselves and which must not be swallowed.
		/// </remarks>
		void SkipToNextPossibleAddress ()
		{
			while (index < endIndex && !IsAddressListSeparator (text[index]) && !CanBeginAddress (text[index]))
				index++;
		}

		/// <summary>
		/// Report a missing address separator, along with the ambiguity it creates when an angle-addr
		/// is involved.
		/// </summary>
		/// <remarks>
		/// An ordinary missing comma, as in <c>a@example.com b@example.com</c>, leaves two bare
		/// addr-specs with no grammatical relationship to each other: a display-name only appears in
		/// a name-addr, which requires angle brackets, so no conformant reading folds the left token
		/// into the right one. Parsers may still disagree about how many mailboxes to recover, but a
		/// parser that recovers a single mailbox has to invent a production the grammar does not
		/// offer. An angle-addr on either side of the gap is different: <c>phrase angle-addr</c> is a
		/// real production, so reading everything in front of the angle-addr as its display-name is a
		/// legitimate application of the grammar rather than a departure from it. Both readings are
		/// then defensible and they disagree about which mailbox the address names rather than merely
		/// about how many there are. That is what makes it worth reporting separately from the syntax
		/// error.
		/// </remarks>
		void LogMissingAddressSeparator (int addressStart)
		{
			Log (MimeComplianceViolation.MissingAddressSeparator, index);

			if (text[index] == (byte) '<' || ContainsAngleAddr (addressStart, index))
				Log (MimeComplianceViolation.AmbiguousMailboxBoundary, index);
		}

		/// <summary>
		/// Determine whether the given range contains a '&lt;' that opens an angle-addr.
		/// </summary>
		/// <remarks>
		/// Quoted-strings and comments are skipped, because a '&lt;' inside either of them is
		/// ordinary text rather than the start of an angle-addr.
		/// </remarks>
		bool ContainsAngleAddr (int start, int end)
		{
			for (int i = start; i < end; i++) {
				if (text[i] == (byte) '"') {
					for (i++; i < end; i++) {
						if (text[i] == (byte) '\\')
							i++;
						else if (text[i] == (byte) '"')
							break;
					}
				} else if (text[i] == (byte) '(') {
					int depth = 1;

					for (i++; i < end && depth > 0; i++) {
						if (text[i] == (byte) '\\')
							i++;
						else if (text[i] == (byte) '(')
							depth++;
						else if (text[i] == (byte) ')')
							depth--;
					}

					i--;
				} else if (text[i] == (byte) '<') {
					return true;
				}
			}

			return false;
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

			bool reportedInvalidCharacter = false;
			bool reportedWhiteSpace = false;

			while (index < endIndex && text[index] != (byte) ']') {
				// Note: A lone carriage return is whitespace, but ScanForControlCharacters has already
				// reported it as a control character, which is the more accurate description of it.
				if (text[index].IsWhitespace () && !reportedWhiteSpace && !IsControlCharacter (index)) {
					Log (MimeComplianceViolation.WhitespaceInDomainLiteral, index);
					reportedWhiteSpace = true;
				} else if ((text[index] == (byte) '[' || text[index] == (byte) '\\') && !reportedInvalidCharacter) {
					// Note: These are the only two characters dtext excludes that can reach this loop. A ']'
					// ends it, control characters and invalid 8-bit bytes have already been reported by their
					// character class, and rfc6532 adds the remaining 8-bit bytes to dtext.
					Log (MimeComplianceViolation.InvalidCharacterInDomainLiteral, index);
					reportedInvalidCharacter = true;
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
				int localPartStart = index;
				bool closed;

				// local-part = dot-atom / quoted-string
				if (index < endIndex && text[index] == (byte) '"') {
					int quoted = index;

					closed = SkipQuoted ();

					// Note: Section 3.2.4 of rfc5322 does permit FWS inside a quoted-string, so this
					// one is reported despite being legal. A quoted local-part is rare enough that
					// implementations get it wrong, and both MimeKit and Exchange have mishandled a
					// line break here, so the divergence this violation exists to report is real
					// whether or not the grammar allows the construct.
					if (closed)
						CheckLineBreak (quoted, index, ref reportedLineBreak);
				} else {
					closed = true;

					ValidateDotAtom (MimeComplianceViolation.InvalidLocalPart);
				}

				// Note: The local-part is attributed as soon as its token has been consumed, rather
				// than once the whole addr-spec is known, so that the attribution survives a
				// quoted-string that was never closed and so that trailing comments are excluded.
				CheckIso2022LocalPart (localPartStart, index);

				if (!closed)
					return;

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
					LogMissingAddressSeparator (before);
					SkipToNextPossibleAddress ();
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
				} else if (ContainsAddrspec (start, index)) {
					Log (MimeComplianceViolation.AddressInGroupDisplayName, start);
				}

				ValidateGroup (unquotedSpecial, specialIndex);
				return true;
			}

			if (c == (byte) '<') {
				if (unquotedSpecial)
					Log (MimeComplianceViolation.UnquotedDisplayName, specialIndex);

				if (hasContent && ContainsAddrspec (start, index))
					Log (MimeComplianceViolation.AddressInDisplayName, start);

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
		/// Determine whether a phrase used as a display-name is itself shaped like an addr-spec.
		/// </summary>
		/// <remarks>
		/// This looks inside quoted-strings, because <c>"admin@example.com" &lt;attacker@example.org&gt;</c>
		/// is the whole point: the quoting makes the value legal, not innocuous. Comments are skipped,
		/// and an '@' only counts when it has atom text immediately before it and a dotted domain
		/// immediately after it, so that phrases such as <c>Bob @ Work</c> and <c>@channel</c> are not
		/// mistaken for addresses.
		/// </remarks>
		bool ContainsAddrspec (int start, int end)
		{
			bool precededByAtom = false;
			bool quoted = false;
			int i = start;

			while (i < end) {
				byte c = text[i];

				if (c == (byte) '"') {
					quoted = !quoted;
					precededByAtom = false;
					i++;
					continue;
				}

				if (c == (byte) '(' && !quoted) {
					int depth = 1;

					for (i++; i < end && depth > 0; i++) {
						if (text[i] == (byte) '\\')
							i++;
						else if (text[i] == (byte) '(')
							depth++;
						else if (text[i] == (byte) ')')
							depth--;
					}

					precededByAtom = false;
					continue;
				}

				if (c == (byte) '\\' && i + 1 < end)
					c = text[++i];

				if (c == (byte) '@' && precededByAtom && IsDottedDomain (i + 1, end))
					return true;

				precededByAtom = c.IsAtom ();
				i++;
			}

			return false;
		}

		/// <summary>
		/// Determine whether a dot-atom containing at least one dot begins at the given offset.
		/// </summary>
		/// <remarks>
		/// Requiring a dot keeps a local-part followed by a bare word, such as <c>user@work</c>, from
		/// being read as an address. That is deliberately conservative: a display-name is free-form
		/// text, so the cost of a false positive is higher than the cost of missing a domain that no
		/// public mail system would route to anyway.
		/// </remarks>
		bool IsDottedDomain (int i, int end)
		{
			bool atom = false, dot = false, atomAfterDot = false;

			while (i < end) {
				byte c = text[i];

				if (c.IsAtom ()) {
					atom = true;

					if (dot)
						atomAfterDot = true;
				} else if (c == (byte) '.') {
					if (!atom)
						return false;

					dot = true;
				} else {
					break;
				}

				i++;
			}

			return atom && dot && atomAfterDot;
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

			iso2022?.Clear ();
			reportedControlCharacter = false;
			reportedStrayCarriageReturn = false;
			reportedIso2022LocalPart = false;

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

			ValidateAddressList ();

			// Note: This has to run after the parse, because until then there is no telling which of
			// the recorded ISO-2022 sequences landed in a local-part.
			ReportUnattributedIso2022Sequences ();
		}

		/// <summary>
		/// Validate the address-list that the validator has been positioned over.
		/// </summary>
		/// <remarks>
		/// This is split out from <see cref="Validate"/> so that the caller can always run the
		/// post-parse passes, no matter which of the early returns below ends the parse.
		/// </remarks>
		void ValidateAddressList ()
		{
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
					LogMissingAddressSeparator (before);
					SkipToNextPossibleAddress ();
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
