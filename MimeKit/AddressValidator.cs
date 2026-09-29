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
#if NET8_0_OR_GREATER
using System.Buffers;
#endif

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
#if NET8_0_OR_GREATER
		// Note: Every byte that ScanForControlCharacters has anything to say about: a null, a
		// shift-out, a shift-in or an escape introducing an ISO-2022 sequence, a carriage return, and
		// any other C0 or delete character. Tab and linefeed are excluded because folding whitespace
		// is legal between address tokens. Nothing at or above 0x20 belongs here -- in particular the
		// 8-bit range is untouched, since rfc6532 makes it ordinary address text -- so a value of
		// real-world mail almost never contains one of these and the whole scan collapses into a
		// handful of vector comparisons.
		static readonly SearchValues<byte> ControlCharacters = SearchValues.Create ([
			0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, /* 0x09 tab */ /* 0x0a linefeed */ 0x0b,
			0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
			0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x7f
		]);

		// Note: The complement of IsAtomOrControl, which is what SkipAtom stops on. It is derived from
		// that method rather than written out so that the two cannot drift apart.
		static readonly SearchValues<byte> NotAtomOrControl = SearchValues.Create (Complement (IsAtomOrControl));

		// Note: The complement of the bytes a phrase can be built out of, which is where an unquoted
		// special begins, and the complement of folding whitespace, which is where a phrase's first
		// word begins. Both are derived from the same predicates the token scan uses.
		static readonly SearchValues<byte> NotPhraseText = SearchValues.Create (Complement (c => IsAtomOrControl (c) || c.IsWhitespace ()));
		static readonly SearchValues<byte> NotWhiteSpace = SearchValues.Create (Complement (c => c.IsWhitespace ()));

		// Note: The bytes that decide the shape of a phrase: the ones that open a quoted-string, a
		// comment or a domain-literal, and the ones that can terminate the phrase. Everything else --
		// atom text, whitespace, '@', '.' and the bytes no production can consume -- is passed over
		// without changing the outcome, so finding the first of these answers "is this token a bare
		// addr-spec?" in a single vectorized scan.
		static readonly SearchValues<byte> PhraseStructure = SearchValues.Create ([
			(byte) '"', (byte) '(', (byte) '[', (byte) '<', (byte) ':', (byte) ',', (byte) ';', (byte) '>'
		]);

		static byte[] Complement (Func<byte, bool> predicate)
		{
			var bytes = new byte[256];
			int n = 0;

			for (int c = 0; c < 256; c++) {
				if (!predicate ((byte) c))
					bytes[n++] = (byte) c;
			}

			Array.Resize (ref bytes, n);

			return bytes;
		}
#endif

		readonly IMimeComplianceLogger logger;
		readonly MimeComplianceContext context;
		readonly long streamOffset;
		readonly int lineNumber;
		readonly int columnNumber;
		byte[] text;
		int startIndex;
		int endIndex;
		int index;

		// Note: The position of the first ISO-2022 shift or escape sequence that has not yet turned out
		// to be inside a local-part, or -1 once none are left. Sequences are held back rather than
		// reported where they are found because how one should be described depends on where it turns
		// out to be, which is not known until the value has been parsed. Only the first survivor is
		// ever reported, and local-parts are visited in increasing order, so a cursor that moves
		// forward as each is claimed is all the bookkeeping that is needed.
		int iso2022Cursor;

		// Note: The position of the first null byte that has not yet turned out to be inside a
		// display-name, or -1 once none are left. Held back and advanced for the same reasons as the
		// ISO-2022 cursor above: a null in a display-name is described differently from one in an
		// addr-spec, and which it is cannot be known until the value has been parsed.
		int nullCursor;
		bool reportedControlCharacter;
		bool reportedStrayCarriageReturn;
		bool reportedIso2022LocalPart;
		bool reportedNullByteInDisplayName;
		bool reportedNullByteInAddress;

		// Note: Whether the address currently being scanned was cut short by a damaged local-part.
		// Set by LogDamagedLocalPart and cleared at the start of each address, it tells the list loops
		// that the character the scan stopped on has already been accounted for.
		bool damagedLocalPart;

		// Note: Whether scanning is inside a group-list. A ';' ends a group, but rfc5322 address-list
		// is comma-separated only, so outside a group there is nothing for a ';' to delimit and it is
		// an unquoted special in whatever token it interrupted.
		bool inGroupList;

		// Note: Where the last call to Log finished working out a position, so that the next one can
		// resume from there instead of counting the lines over again from the start of the value.
		// Violations come out in increasing order of position almost always, and a value that
		// produces a lot of them would otherwise cost O(length x violations) just to describe.
		int scanIndex;
		int scanLine;
		int scanLineBegin;

		// Note: The extent of a phrase that has already been scanned, and which any later scan
		// beginning inside it is guaranteed to agree with. See RecordPhraseExtent().
		bool skippedComment;
		int phraseFrom;
		int phraseEnd;

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
			if (at < scanIndex) {
				// Note: A violation behind the last one, such as a group terminator reported at the
				// ':' that opened the group once the end of the value has been reached. Rare enough
				// that starting the scan over costs less than remembering where every line began.
				scanIndex = startIndex;
				scanLine = lineNumber;
				scanLineBegin = -1;
			}

			int end = Math.Min (at, endIndex);

			for (int i = scanIndex; i < end; i++) {
				if (text[i] == (byte) '\n') {
					scanLine++;
					scanLineBegin = i + 1;
				}
			}

			scanIndex = end;

			// Note: Until the value has been folded, the column is still relative to the field name
			// that preceded it on the same line.
			int column = scanLineBegin < 0 ? columnNumber + (at - startIndex) : (at - scanLineBegin) + 1;

			logger.Log (new MimeComplianceIssue (context, violation, streamOffset + (at - startIndex), scanLine, column));
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

				skippedComment = true;

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
		/// <para>Null bytes are recorded rather than reported for the same reason: one in a display-name
		/// is described differently from one in an addr-spec, so the decision is deferred to
		/// <see cref="CheckNullBytesInDisplayName"/> and <see cref="ReportUnattributedNullBytes"/>.</para>
		/// </remarks>
		void ScanForControlCharacters ()
		{
			// Note: Only the position of the first null byte and of the first ISO-2022 sequence is kept.
			// Each class is reported at most once, and the cursors are advanced by searching forward
			// again if the token they land in turns out to account for them, so there is nothing to be
			// gained by remembering the rest.
			for (int i = startIndex; i < endIndex; i++) {
#if NET8_0_OR_GREATER
				// Note: Nothing below has anything to say about a byte outside ControlCharacters, and
				// a header value of ordinary mail consists of nothing else, so jump straight to the
				// next byte that could matter rather than examining each one.
				int next = new ReadOnlySpan<byte> (text, i, endIndex - i).IndexOfAny (ControlCharacters);

				if (next < 0)
					break;

				i += next;
#endif
				byte c = text[i];

				if (c == 0) {
					if (nullCursor < 0)
						nullCursor = i;
				} else if (IsIso2022Sequence (i, out int length)) {
					if (iso2022Cursor < 0)
						iso2022Cursor = i;

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
		/// Find the next ISO-2022 shift or escape sequence beginning at or after <paramref name="from"/>
		/// and before <paramref name="limit"/>, or -1 if there is none.
		/// </summary>
		/// <remarks>
		/// No sequence can begin inside another, because the intermediate and final bytes of one are
		/// drawn from ranges that contain none of the bytes that can start one, so it is safe to begin
		/// looking from an arbitrary position.
		/// </remarks>
		int NextIso2022Sequence (int from, int limit)
		{
			while (from < limit) {
				int at = new ReadOnlySpan<byte> (text, from, limit - from).IndexOfAny ((byte) 0x0e, (byte) 0x0f, (byte) 0x1b);

				if (at < 0)
					break;

				from += at;

				if (IsIso2022Sequence (from, out int length))
					return from;

				from += Math.Max (length, 1);
			}

			return -1;
		}

		/// <summary>
		/// Find the next null byte at or after <paramref name="from"/> and before
		/// <paramref name="limit"/>, or -1 if there is none.
		/// </summary>
		int NextNullByte (int from, int limit)
		{
			if (from >= limit)
				return -1;

			int at = new ReadOnlySpan<byte> (text, from, limit - from).IndexOf ((byte) 0);

			return at < 0 ? -1 : from + at;
		}

		/// <summary>
		/// Attribute any ISO-2022 sequence that falls within the given range to the local-part that
		/// occupies it.
		/// </summary>
		/// <remarks>
		/// A sequence claimed here is struck from the pending set so that the same bytes are not also
		/// described as control characters by <see cref="ReportUnattributedIso2022Sequences"/>.
		/// </remarks>
		/// <param name="start">The start of the local-part.</param>
		/// <param name="end">The end of the local-part.</param>
		void CheckIso2022LocalPart (int start, int end)
		{
			if (iso2022Cursor < 0)
				return;

			// Note: Only the first local-part sequence is reported, so once one has been found the
			// range no longer needs to be examined at all.
			if (!reportedIso2022LocalPart) {
				int at = NextIso2022Sequence (start, end);

				if (at >= 0) {
					Log (MimeComplianceViolation.Iso2022SequenceInLocalPart, at);
					reportedIso2022LocalPart = true;
				}
			}

			// Note: Local-parts are visited in increasing order, so a pending sequence that this one
			// accounts for can never be wanted again and the cursor only ever moves forward.
			if (iso2022Cursor >= start && iso2022Cursor < end)
				iso2022Cursor = NextIso2022Sequence (end, endIndex);
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
			if (iso2022Cursor < 0 || reportedControlCharacter)
				return;

			Log (MimeComplianceViolation.ControlCharacterInAddress, iso2022Cursor);
			reportedControlCharacter = true;
		}

		/// <summary>
		/// Attribute any null byte that falls within the given range to the display-name that occupies
		/// it.
		/// </summary>
		/// <remarks>
		/// A null byte claimed here is struck from the pending set so that the same byte is not also
		/// described as an addr-spec null by <see cref="ReportUnattributedNullBytes"/>.
		/// </remarks>
		/// <param name="start">The start of the display-name.</param>
		/// <param name="end">The end of the display-name.</param>
		void CheckNullBytesInDisplayName (int start, int end)
		{
			if (nullCursor < 0)
				return;

			// Note: Only the first display-name null is reported, so once one has been found the range
			// no longer needs to be examined at all.
			if (!reportedNullByteInDisplayName) {
				int at = NextNullByte (start, end);

				if (at >= 0) {
					Log (MimeComplianceViolation.NullByteInDisplayName, at);
					reportedNullByteInDisplayName = true;
				}
			}

			// Note: Display-names are visited in increasing order, so a pending null that this one
			// accounts for can never be wanted again and the cursor only ever moves forward.
			if (nullCursor >= start && nullCursor < end)
				nullCursor = NextNullByte (end, endIndex);
		}

		/// <summary>
		/// Report the first null byte that did not turn out to be inside a display-name as an addr-spec
		/// null byte.
		/// </summary>
		/// <remarks>
		/// This has to run after the parse, because nothing before it can say where a null byte ended
		/// up, but it runs on every exit path so that the finding still survives a parse that gave up
		/// early. A null byte that the parse never reached is reported here too: it is not in any
		/// display-name, and the addr-spec description is the more serious of the two.
		/// </remarks>
		void ReportUnattributedNullBytes ()
		{
			if (nullCursor < 0 || reportedNullByteInAddress)
				return;

			Log (MimeComplianceViolation.NullByteInAddress, nullCursor);
			reportedNullByteInAddress = true;
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
		/// Report a local-part that was cut short by a character no addr-spec production can contain.
		/// </summary>
		/// <remarks>
		/// <para>The local-part ended somewhere other than at an <c>@</c>, a list delimiter or the end of
		/// the value, which means an unquoted special landed inside it: <c>a[b@example.com</c>,
		/// <c>a\b@example.com</c>, <c>a b@example.com</c>. Quoting it, as section 3.4.1 of rfc5322
		/// requires, would have made all of these legal.</para>
		/// <para>This also suppresses the missing-separator report for the address, because the two
		/// findings are mutually exclusive descriptions of the same character rather than two faults.
		/// The scan stops mid-token, so the character it stopped on is whatever damaged the local-part,
		/// not the place a comma was left out -- there is no second address for a comma to have
		/// separated.</para>
		/// <para>Resynchronization steps over that character when it is a list separator, since
		/// <see cref="SkipToNextPossibleAddress"/> halts on one and would otherwise hand the same
		/// already-reported byte back to the list loop as the start of another address. Anything else
		/// it can skip on its own.</para>
		/// </remarks>
		/// <param name="at">The offset of the character that ended the local-part.</param>
		void LogDamagedLocalPart (int at)
		{
			Log (MimeComplianceViolation.InvalidLocalPart, at);
			damagedLocalPart = true;
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

#if NET8_0_OR_GREATER
			int at = new ReadOnlySpan<byte> (text, index, endIndex - index).IndexOfAny (NotAtomOrControl);

			index = at < 0 ? endIndex : index + at;
#else
			while (index < endIndex && IsAtomOrControl (text[index]))
				index++;
#endif

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
				if (index >= endIndex || text[index] == (byte) ',' || (inGroupList && text[index] == (byte) ';') || (inAngleAddr && text[index] == (byte) '>'))
					Log (MimeComplianceViolation.AddressWithoutDomain, start);
				else
					LogDamagedLocalPart (index);

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
		/// Remember how far a phrase scan reached, if a later scan can be trusted to agree with it.
		/// </summary>
		/// <remarks>
		/// <para>A phrase scan runs forward until it finds something that could terminate a
		/// display-name, so a list of bare addr-specs with nothing separating them re-scans the whole
		/// remainder of the value once per address, which costs O(length squared).</para>
		/// <para>When the region just scanned contained no quoted-string, no domain-literal and no
		/// comment, every byte in it is one the scan walked straight over rather than skipped as a
		/// unit, so a scan beginning anywhere inside it must walk over the rest of those same bytes
		/// and stop in the same place. That makes the extent reusable.</para>
		/// <para>The extent is only recorded when the terminator is neither '&lt;' nor ':', because
		/// those are the only two that make <see cref="ValidateAddress"/> look at what the phrase
		/// contained rather than merely how far it ran, and the recorded extent deliberately says
		/// nothing about the contents.</para>
		/// </remarks>
		/// <param name="start">The index the phrase scan began at.</param>
		/// <param name="simple">Whether the scanned region contained no quoted-string, domain-literal or comment.</param>
		/// <param name="reusableTerminator">Whether the scan stopped on something other than '&lt;' or ':'.</param>
		void RecordPhraseExtent (int start, bool simple, bool reusableTerminator)
		{
			if (simple && reusableTerminator && index > start) {
				phraseFrom = start;
				phraseEnd = index;
			}
		}

		/// <summary>
		/// Try to scan a phrase without walking it token by token, returning <see langword="false"/> if
		/// it holds something that only the full scan can make sense of.
		/// </summary>
		/// <remarks>
		/// <para>A leading token has to be scanned before it is known whether it is a display-name or an
		/// addr-spec, and <see cref="ScanPhrase"/> pays for that by walking every token in it. Almost
		/// none of that walk is load-bearing: of everything it looks at, only a quoted-string, a comment
		/// or a domain-literal can hide the terminator, and only '&lt;' and ':' select a path that reads
		/// what it recorded along the way.</para>
		/// <para>So the first byte from either of those two groups decides everything. If it opens a
		/// quoted-string, a comment or a domain-literal, this gives up and the full scan runs. If it is
		/// a ',', ';' or '&gt;' -- or there is none at all -- the phrase is really an addr-spec, which
		/// <see cref="ValidateAddrspec"/> is about to re-read from the beginning anyway, so nothing more
		/// needs to be worked out here. Otherwise the phrase is a display-name made only of atoms,
		/// whitespace and unquoted specials, and both findings about it can be had from a further scan
		/// apiece rather than from a walk.</para>
		/// </remarks>
		/// <param name="unquotedSpecial">Whether the phrase contained a character that would have had to be quoted.</param>
		/// <param name="specialIndex">The index of the first such character.</param>
		/// <param name="hasContent">Whether the phrase contained anything at all besides comments and whitespace.</param>
		bool TryScanPhrase (out bool unquotedSpecial, out int specialIndex, out bool hasContent)
		{
			unquotedSpecial = false;
			specialIndex = -1;
			hasContent = false;

#if NET8_0_OR_GREATER
			int start = index;
			var remaining = new ReadOnlySpan<byte> (text, index, endIndex - index);
			int at = remaining.IndexOfAny (PhraseStructure);

			if (at < 0) {
				index = endIndex;
				skippedComment = false;
				RecordPhraseExtent (start, true, true);
				return true;
			}

			byte c = text[index + at];

			if (c == (byte) '"' || c == (byte) '(' || c == (byte) '[')
				return false;

			index += at;
			skippedComment = false;

			if (c != (byte) '<' && c != (byte) ':') {
				// Note: A ',', ';' or '>' means the phrase was an addr-spec, and ValidateAddress reads
				// none of the findings below on those paths. They are left at their defaults rather
				// than computed so that the common case costs a single scan.
				RecordPhraseExtent (start, true, true);
				return true;
			}

			var phrase = remaining.Slice (0, at);
			int special = phrase.IndexOfAny (NotPhraseText);

			if (special >= 0) {
				unquotedSpecial = true;
				specialIndex = start + special;
				hasContent = true;
			} else {
				hasContent = phrase.IndexOfAny (NotWhiteSpace) >= 0;
			}

			// Note: RecordPhraseExtent is deliberately not called. A '<' or ':' terminator makes the
			// extent unusable as a memo; see RecordPhraseExtent for why.

			return true;
#else
			return false;
#endif
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
			int start = index;
			bool simple = true;

			unquotedSpecial = false;
			specialIndex = -1;
			hasContent = false;
			skippedComment = false;

			do {
				if (!SkipCFWS ())
					return false;

				if (skippedComment)
					simple = false;

				if (index >= endIndex) {
					RecordPhraseExtent (start, simple, true);
					return true;
				}

				byte c = text[index];

				if (c == (byte) '"') {
					hasContent = true;
					simple = false;

					if (!SkipQuoted ())
						return false;
				} else if (c == (byte) '<' || c == (byte) ':' || c == (byte) ',' || c == (byte) ';' || c == (byte) '>') {
					RecordPhraseExtent (start, simple, c != (byte) '<' && c != (byte) ':');
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
					simple = false;

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

			bool wasInGroupList = inGroupList;

			inGroupList = true;

			try {
				ValidateGroupList (start);
			} finally {
				inGroupList = wasInGroupList;
			}
		}

		void ValidateGroupList (int start)
		{
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

				damagedLocalPart = false;

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
					if (!damagedLocalPart)
						LogMissingAddressSeparator (before);
					else if (IsAddressListSeparator (text[index]))
						index++;

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
			bool unquotedSpecial = false;
			bool hasContent = true;
			int specialIndex = -1;

			if (start >= phraseFrom && start < phraseEnd) {
				// Note: The phrase ahead has already been scanned and is known to end here. See
				// RecordPhraseExtent() for why this is safe and why ':' and '<' cannot reach it.
				index = phraseEnd;
			} else if (!TryScanPhrase (out unquotedSpecial, out specialIndex, out hasContent) && !ScanPhrase (out unquotedSpecial, out specialIndex, out hasContent)) {
				return false;
			}

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

				CheckNullBytesInDisplayName (start, index);

				ValidateGroup (unquotedSpecial, specialIndex);
				return true;
			}

			if (c == (byte) '<') {
				if (unquotedSpecial)
					Log (MimeComplianceViolation.UnquotedDisplayName, specialIndex);

				if (hasContent && ContainsAddrspec (start, index))
					Log (MimeComplianceViolation.AddressInDisplayName, start);

				CheckNullBytesInDisplayName (start, index);

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

			iso2022Cursor = -1;
			nullCursor = -1;
			reportedControlCharacter = false;
			reportedStrayCarriageReturn = false;
			reportedIso2022LocalPart = false;
			reportedNullByteInDisplayName = false;
			reportedNullByteInAddress = false;
			damagedLocalPart = false;
			inGroupList = false;
			scanIndex = startIndex;
			scanLine = lineNumber;
			scanLineBegin = -1;
			phraseFrom = -1;
			phraseEnd = -1;

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

			// Note: These have to run after the parse, because until then there is no telling which of
			// the recorded ISO-2022 sequences landed in a local-part, or which null bytes landed in a
			// display-name.
			ReportUnattributedIso2022Sequences ();
			ReportUnattributedNullBytes ();
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

				damagedLocalPart = false;

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
					if (damagedLocalPart) {
						// The offending character has been reported already. Step over it if it is
						// a list separator, which SkipToNextPossibleAddress halts on rather than
						// skips past.
						if (IsAddressListSeparator (text[index]))
							index++;
					} else if (text[index] == (byte) ';') {
						// A ';' does not separate the elements of an address-list, it terminates a
						// group, and there is no group open here. Reporting a missing comma would
						// describe the same byte a second time and in the wrong terms: nothing is
						// missing, something extra is present. The top of the loop has a report for
						// exactly that and consumes the byte, so leave it to do both.
						continue;
					} else {
						LogMissingAddressSeparator (before);
					}

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
