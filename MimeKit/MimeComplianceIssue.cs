//
// MimeComplianceIssue.cs
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
using System.Globalization;

namespace MimeKit {
	/// <summary>
	/// A MIME compliance issue detected while parsing.
	/// </summary>
	/// <remarks>
	/// <para>Describes a single deviation from the Internet Message and MIME specifications along
	/// with the location within the stream where it was detected.</para>
	/// <para>New properties may be added to this structure in future versions of MimeKit in order to
	/// provide richer context about a violation. For that reason, always construct instances using one
	/// of the available constructors rather than relying on the default value.</para>
	/// <para>The default value of this structure has a <see cref="Violation"/> of
	/// <see cref="MimeComplianceViolation.None"/>, which is never reported by <see cref="MimeReader"/>
	/// and cannot be constructed, so it may be used to detect an uninitialized instance.</para>
	/// </remarks>
	public readonly struct MimeComplianceIssue : IEquatable<MimeComplianceIssue>
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MimeComplianceIssue"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeComplianceIssue"/> that will be rated for the specified context.
		/// </remarks>
		/// <param name="context">The context that the message is being used in.</param>
		/// <param name="violation">The specific MIME compliance violation that occurred.</param>
		/// <param name="streamOffset">The offset within the stream where the violation was found.</param>
		/// <param name="lineNumber">The one-based line number where the violation was found.</param>
		/// <param name="columnNumber">The one-based column number where the violation was found, or <c>0</c> if unknown.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="context"/> is not a valid <see cref="MimeComplianceContext"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="violation"/> is not a valid <see cref="MimeComplianceViolation"/>.</para>
		/// </exception>
		public MimeComplianceIssue (MimeComplianceContext context, MimeComplianceViolation violation, long streamOffset, int lineNumber, int columnNumber) : this (context, violation, streamOffset, lineNumber, columnNumber, MimeCompliancePositionKind.Exact)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeComplianceIssue"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeComplianceIssue"/> that will be rated for the specified context.
		/// </remarks>
		/// <param name="context">The context that the message is being used in.</param>
		/// <param name="violation">The specific MIME compliance violation that occurred.</param>
		/// <param name="streamOffset">The offset within the stream where the violation was found.</param>
		/// <param name="lineNumber">The one-based line number where the violation was found.</param>
		/// <param name="columnNumber">The one-based column number where the violation was found, or <c>0</c> if unknown.</param>
		/// <param name="positionKind">What the position refers to.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="context"/> is not a valid <see cref="MimeComplianceContext"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="violation"/> is not a valid <see cref="MimeComplianceViolation"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="positionKind"/> is not a valid <see cref="MimeCompliancePositionKind"/>.</para>
		/// </exception>
		public MimeComplianceIssue (MimeComplianceContext context, MimeComplianceViolation violation, long streamOffset, int lineNumber, int columnNumber, MimeCompliancePositionKind positionKind)
		{
			if (context != MimeComplianceContext.Transport && context != MimeComplianceContext.Storage)
				throw new ArgumentOutOfRangeException (nameof (context));

			if (violation <= MimeComplianceViolation.None || violation > MimeComplianceViolation.IncompleteUUEncodedContent)
				throw new ArgumentOutOfRangeException (nameof (violation));

			if (positionKind < MimeCompliancePositionKind.Exact || positionKind > MimeCompliancePositionKind.ElementStart)
				throw new ArgumentOutOfRangeException (nameof (positionKind));

			Context = context;
			Violation = violation;
			StreamOffset = streamOffset;
			LineNumber = lineNumber;
			ColumnNumber = columnNumber;
			PositionKind = positionKind;
		}

		/// <summary>
		/// Get the context that the message is being used in.
		/// </summary>
		/// <remarks>
		/// <para>Gets the context that the message is being used in, which determines how
		/// <see cref="Severity"/> rates the violation.</para>
		/// <para>When an issue is reported by <see cref="MimeReader"/>, this is the value of
		/// <see cref="MimeReader.ComplianceContext"/> at the time that the violation was detected.</para>
		/// </remarks>
		/// <value>The context.</value>
		public MimeComplianceContext Context {
			get;
		}

		/// <summary>
		/// Get the specific MIME compliance violation that occurred.
		/// </summary>
		/// <remarks>
		/// Gets the specific MIME compliance violation that occurred.
		/// </remarks>
		/// <value>The MIME compliance violation.</value>
		public MimeComplianceViolation Violation {
			get;
		}

		/// <summary>
		/// Get the offset within the stream where the violation was found.
		/// </summary>
		/// <remarks>
		/// Gets the offset within the stream where the violation was found.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long StreamOffset {
			get;
		}

		/// <summary>
		/// Get the line number where the violation was found.
		/// </summary>
		/// <remarks>
		/// Gets the one-based line number where the violation was found.
		/// </remarks>
		/// <value>The line number.</value>
		public int LineNumber {
			get;
		}

		/// <summary>
		/// Get the column number where the violation was found.
		/// </summary>
		/// <remarks>
		/// Gets the one-based column number where the violation was found, or <c>0</c> if the column
		/// is unknown.
		/// </remarks>
		/// <value>The column number.</value>
		public int ColumnNumber {
			get;
		}

		/// <summary>
		/// Get what the position of the issue refers to.
		/// </summary>
		/// <remarks>
		/// <para>Gets whether <see cref="LineNumber"/> and <see cref="ColumnNumber"/> identify the
		/// offending byte itself or merely the start of the line or element that contains it.</para>
		/// <para>A tool that renders diagnostics can use this to decide whether it needs to search
		/// for the offending byte, and how far.</para>
		/// </remarks>
		/// <value>What the position refers to.</value>
		public MimeCompliancePositionKind PositionKind {
			get;
		}

		/// <summary>
		/// Get the severity of the MIME compliance violation.
		/// </summary>
		/// <remarks>
		/// <para>Gets how much practical harm the violation is likely to cause when the message is
		/// used in <see cref="Context"/>.</para>
		/// <para>Use <see cref="GetSeverity(MimeComplianceContext)"/> to rate the violation for a
		/// different context than the one that the issue was reported for.</para>
		/// </remarks>
		/// <value>The severity.</value>
		public MimeComplianceSeverity Severity {
			get { return GetSeverity (Violation, Context); }
		}

		/// <summary>
		/// Get the severity of the MIME compliance violation in a particular context.
		/// </summary>
		/// <remarks>
		/// Gets how much practical harm the violation is likely to cause when the message is used in
		/// the specified context. Use this instead of <see cref="Severity"/> to rate the violation
		/// for a different context than the one that the issue was reported for.
		/// </remarks>
		/// <returns>The severity.</returns>
		/// <param name="context">The context that the message is being used in.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="context"/> is not a valid <see cref="MimeComplianceContext"/>.
		/// </exception>
		public MimeComplianceSeverity GetSeverity (MimeComplianceContext context)
		{
			return GetSeverity (Violation, context);
		}

		/// <summary>
		/// Get the categories of harm that the MIME compliance violation may cause.
		/// </summary>
		/// <remarks>
		/// <para>Gets what kind of harm the violation may cause, as opposed to <see cref="Severity"/>,
		/// which rates how much.</para>
		/// <para>A violation may fall into more than one category, so this is a bit field.</para>
		/// </remarks>
		/// <value>The categories.</value>
		public MimeComplianceCategories Categories {
			get { return GetCategories (Violation); }
		}

		/// <summary>
		/// Get a brief description of the MIME compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a brief, single-sentence description of what went wrong. Use <see cref="Remarks"/>
		/// for a longer explanation of why it matters.
		/// </remarks>
		/// <value>The description.</value>
		public string Description {
			get { return GetDescription (Violation); }
		}

		/// <summary>
		/// Get a detailed explanation of the MIME compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a detailed explanation of what the relevant specifications require and the problems
		/// that the violation is likely to cause.
		/// </remarks>
		/// <value>The remarks.</value>
		public string Remarks {
			get { return GetRemarks (Violation); }
		}

		/// <summary>
		/// Get a string representation of the MIME compliance issue.
		/// </summary>
		/// <remarks>
		/// Gets a string representation of the MIME compliance issue.
		/// </remarks>
		/// <returns>A string representation of the MIME compliance issue.</returns>
		public override string ToString ()
		{
			if (ColumnNumber > 0)
				return string.Format (CultureInfo.InvariantCulture, "{0} at line {1}, column {2} (offset {3})", Violation, LineNumber, ColumnNumber, StreamOffset);

			return string.Format (CultureInfo.InvariantCulture, "{0} at line {1} (offset {2})", Violation, LineNumber, StreamOffset);
		}

		/// <summary>
		/// Determine whether the specified <see cref="MimeComplianceIssue"/> is equal to the current <see cref="MimeComplianceIssue"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="MimeComplianceIssue"/> is equal to the current <see cref="MimeComplianceIssue"/>.
		/// </remarks>
		/// <param name="other">The <see cref="MimeComplianceIssue"/> to compare with the current <see cref="MimeComplianceIssue"/>.</param>
		/// <returns><see langword="true" /> if the specified <see cref="MimeComplianceIssue"/> is equal to the current
		/// <see cref="MimeComplianceIssue"/>; otherwise, <see langword="false" />.</returns>
		public bool Equals (MimeComplianceIssue other)
		{
			return other.Context == Context && other.Violation == Violation && other.StreamOffset == StreamOffset &&
				other.LineNumber == LineNumber && other.ColumnNumber == ColumnNumber && other.PositionKind == PositionKind;
		}

		/// <summary>
		/// Determine whether the specified <see cref="System.Object"/> is equal to the current <see cref="MimeComplianceIssue"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="MimeComplianceIssue"/>.
		/// </remarks>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="MimeComplianceIssue"/>.</param>
		/// <returns><see langword="true" /> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="MimeComplianceIssue"/>; otherwise, <see langword="false" />.</returns>
		public override bool Equals (object? obj)
		{
			return obj is MimeComplianceIssue other && Equals (other);
		}

		/// <summary>
		/// Serve as a hash function for a <see cref="MimeComplianceIssue"/> object.
		/// </summary>
		/// <remarks>
		/// Serves as a hash function for a <see cref="MimeComplianceIssue"/> object.
		/// </remarks>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms
		/// and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			return Context.GetHashCode () ^ Violation.GetHashCode () ^ StreamOffset.GetHashCode () ^ LineNumber ^ ColumnNumber ^ PositionKind.GetHashCode ();
		}

		/// <summary>
		/// Compare two <see cref="MimeComplianceIssue"/> objects for equality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="MimeComplianceIssue"/> objects for equality.
		/// </remarks>
		/// <param name="left">The first issue to compare.</param>
		/// <param name="right">The second issue to compare.</param>
		/// <returns><see langword="true" /> if the two issues are equal; otherwise, <see langword="false" />.</returns>
		public static bool operator == (MimeComplianceIssue left, MimeComplianceIssue right)
		{
			return left.Equals (right);
		}

		/// <summary>
		/// Compare two <see cref="MimeComplianceIssue"/> objects for inequality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="MimeComplianceIssue"/> objects for inequality.
		/// </remarks>
		/// <param name="left">The first issue to compare.</param>
		/// <param name="right">The second issue to compare.</param>
		/// <returns><see langword="true" /> if the two issues are not equal; otherwise, <see langword="false" />.</returns>
		public static bool operator != (MimeComplianceIssue left, MimeComplianceIssue right)
		{
			return !left.Equals (right);
		}

		/// <summary>
		/// Get the severity of a MIME compliance violation.
		/// </summary>
		/// <remarks>
		/// <para>Gets how much practical harm the violation is likely to cause, assuming the message
		/// is being transmitted over the network.</para>
		/// <para>Use <see cref="GetSeverity(MimeComplianceViolation,MimeComplianceContext)"/> instead
		/// if the message was read from a local message store, where a few violations are routine and
		/// harmless.</para>
		/// </remarks>
		/// <returns>The severity.</returns>
		/// <param name="violation">The MIME compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="MimeComplianceViolation"/>.
		/// </exception>
		public static MimeComplianceSeverity GetSeverity (MimeComplianceViolation violation)
		{
			return GetSeverity (violation, MimeComplianceContext.Transport);
		}

		/// <summary>
		/// Get the severity of a MIME compliance violation in a particular context.
		/// </summary>
		/// <remarks>
		/// <para>Gets how much practical harm the violation is likely to cause when the message is
		/// used in the specified context.</para>
		/// <para>A handful of violations are requirements of the channel that a message travels over
		/// rather than of the message itself, and are therefore rated lower in
		/// <see cref="MimeComplianceContext.Storage"/> than in
		/// <see cref="MimeComplianceContext.Transport"/>. See <see cref="MimeComplianceContext"/> for
		/// the complete list. Every other violation is rated the same in both contexts.</para>
		/// </remarks>
		/// <returns>The severity.</returns>
		/// <param name="violation">The MIME compliance violation.</param>
		/// <param name="context">The context that the message is being used in.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="violation"/> is not a valid <see cref="MimeComplianceViolation"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="context"/> is not a valid <see cref="MimeComplianceContext"/>.</para>
		/// </exception>
		public static MimeComplianceSeverity GetSeverity (MimeComplianceViolation violation, MimeComplianceContext context)
		{
			if (context != MimeComplianceContext.Transport && context != MimeComplianceContext.Storage)
				throw new ArgumentOutOfRangeException (nameof (context));

			switch (violation) {
			// Note: These are requirements of the channel rather than of the message itself. On the
			// wire they are real problems, but messages in a local store (especially on UNIX systems)
			// routinely exhibit them without any ill effect.
			case MimeComplianceViolation.BareLinefeedInHeader:
			case MimeComplianceViolation.BareLinefeedInBody:
			case MimeComplianceViolation.OversizedLine:
				return context == MimeComplianceContext.Storage ? MimeComplianceSeverity.Minor : MimeComplianceSeverity.Major;

			// Note: 8-bit content is so common that MIME parsers have had to cope with it for
			// decades by falling back to the locale charset or to iso-8859-1 (MimeKit itself tries
			// UTF-8, then the user-supplied charset, then iso-8859-1, which cannot fail). It is
			// therefore no worse on the wire than it is on disk.
			case MimeComplianceViolation.Unexpected8BitBytesInHeader:
			case MimeComplianceViolation.Unexpected8BitBytesInBody:
				return MimeComplianceSeverity.Minor;

			// Note: Duplicate Content-Type and Content-Transfer-Encoding headers and null bytes are
			// the classic MIME "content smuggling" vectors. In each case, a content scanner and an
			// end-user's mail client can be made to disagree about the content of the message.
			case MimeComplianceViolation.RepeatedContentType:
			case MimeComplianceViolation.RepeatedContentTransferEncoding:
			case MimeComplianceViolation.UnexpectedNullBytesInHeader:
			case MimeComplianceViolation.UnexpectedNullBytesInBody:
			// Note: A repeated originator field is the same class of attack applied to the message
			// rather than to a MIME part. An authentication mechanism such as DKIM may cover one
			// instance while the mail client renders another, so the message a filter judges to be
			// safe is not the message the recipient is shown.
			case MimeComplianceViolation.RepeatedDate:
			case MimeComplianceViolation.RepeatedFrom:
			case MimeComplianceViolation.RepeatedSender:
			case MimeComplianceViolation.RepeatedReplyTo:
			case MimeComplianceViolation.RepeatedMessageId:
			case MimeComplianceViolation.RepeatedReturnPath:
				return MimeComplianceSeverity.Critical;

			// Ambiguity or corruption that different MIME parsers may resolve differently.
			case MimeComplianceViolation.InvalidHeader:
			case MimeComplianceViolation.IncompleteHeader:
			case MimeComplianceViolation.InvalidContentType:
			case MimeComplianceViolation.InvalidContentTransferEncoding:
			case MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding:
			case MimeComplianceViolation.IllegalMultipartContentTransferEncoding:
			case MimeComplianceViolation.MissingBodySeparator:
			case MimeComplianceViolation.MissingMultipartBoundaryParameter:
			case MimeComplianceViolation.InvalidMultipartBoundaryParameter:
			case MimeComplianceViolation.MissingMultipartBoundary:
			case MimeComplianceViolation.IncompleteBase64Quantum:
			case MimeComplianceViolation.InvalidBase64Character:
			case MimeComplianceViolation.InvalidBase64Padding:
			case MimeComplianceViolation.Base64CharactersAfterPadding:
			// Note: The characters making up an RFC 1113 comment are themselves valid base64
			// characters, so decoders that do not special-case the '*' delimiters (which is nearly
			// all of them, including MimeKit's own Base64Decoder) silently absorb the comment as
			// content and corrupt everything that follows it.
			case MimeComplianceViolation.ObsoleteBase64Comment:
			case MimeComplianceViolation.InvalidQuotedPrintableEncoding:
			case MimeComplianceViolation.InvalidQuotedPrintableSoftBreak:
			case MimeComplianceViolation.InvalidUUEncodePretext:
			case MimeComplianceViolation.InvalidUUEncodeFileMode:
			case MimeComplianceViolation.InvalidUUEncodedContent:
			case MimeComplianceViolation.InvalidUUEncodedLineLength:
			case MimeComplianceViolation.IncompleteUUEncodedLine:
			case MimeComplianceViolation.InvalidUUEncodedLineExtraData:
			case MimeComplianceViolation.InvalidUUEncodeEndMarker:
			case MimeComplianceViolation.IncompleteUUEncodedContent:
			// Note: A repeated recipient or subject field cannot be used to defeat an
			// authentication mechanism the way a repeated originator field can, but a filter and a
			// mail client can still be made to disagree about who a message was addressed to or
			// what it claims to be about.
			case MimeComplianceViolation.RepeatedTo:
			case MimeComplianceViolation.RepeatedCc:
			case MimeComplianceViolation.RepeatedBcc:
			case MimeComplianceViolation.RepeatedSubject:
				return MimeComplianceSeverity.Major;

			// Note: Obsolete-but-well-defined syntax and defects that MimeKit normalizes away. The
			// resulting address is still the one the author intended; the risk is only that another
			// implementation may normalize differently.
			case MimeComplianceViolation.ObsoleteRouteAddress:
			case MimeComplianceViolation.ExtraneousCommaInAddressList:
			case MimeComplianceViolation.ObsoleteDomainSyntax:
			case MimeComplianceViolation.TrailingDotInDomain:
			case MimeComplianceViolation.WhitespaceInDomainLiteral:
			// Note: Unlike unbalanced brackets, a repeated bracket leaves no doubt about where the
			// address begins and ends. Discarding the extras is trivial and every implementation
			// that does so arrives at the same mailbox.
			case MimeComplianceViolation.ExcessiveAngleBracketsInAddress:
			// Note: These affect only how a message is threaded or how its resending history is
			// displayed. Resent fields in particular are strictly informational and must not be
			// used when processing replies, so a repeated one cannot change where a reply is sent.
			case MimeComplianceViolation.RepeatedInReplyTo:
			case MimeComplianceViolation.RepeatedReferences:
			case MimeComplianceViolation.RepeatedResentDate:
			case MimeComplianceViolation.RepeatedResentFrom:
			case MimeComplianceViolation.RepeatedResentSender:
			case MimeComplianceViolation.RepeatedResentTo:
			case MimeComplianceViolation.RepeatedResentCc:
			case MimeComplianceViolation.RepeatedResentBcc:
			case MimeComplianceViolation.RepeatedResentMessageId:
				return MimeComplianceSeverity.Minor;

			// Note: Ambiguity over where an address begins and ends, or over which mailbox it names.
			case MimeComplianceViolation.UnbalancedAngleBracketsInAddress:
			case MimeComplianceViolation.UnquotedDisplayName:
			case MimeComplianceViolation.InvalidLocalPart:
			case MimeComplianceViolation.InvalidDomain:
			case MimeComplianceViolation.MissingAddressSeparator:
			case MimeComplianceViolation.AmbiguousMailboxBoundary:
			case MimeComplianceViolation.AddressWithoutDomain:
			case MimeComplianceViolation.InvalidCharacterInDomainLiteral:
			case MimeComplianceViolation.Invalid8BitAddress:
			case MimeComplianceViolation.MissingGroupTerminator:
			case MimeComplianceViolation.NonConformantAddress:
				return MimeComplianceSeverity.Major;

			// Note: The input is legal rfc5322, so this rates as a hint rather than as damage. The
			// harm is entirely in what software does with the display-name, which is why the
			// severity is low but the Security category still applies.
			case MimeComplianceViolation.AddressInDisplayName:
			case MimeComplianceViolation.AddressInGroupDisplayName:
				return MimeComplianceSeverity.Minor;

			// Note: Both of these are injection primitives rather than mere syntax errors. A null
			// byte truncates the address for anything that treats it as a C string, and a line break
			// inside an address token is the header/command injection technique that unvalidated
			// input concatenated into a header produces. In each case two components can be made to
			// read different mailboxes, or different message boundaries, out of the same header.
			case MimeComplianceViolation.NullByteInAddress:
			case MimeComplianceViolation.NullByteInDisplayName:
			case MimeComplianceViolation.LineBreakInAddress:
			// Note: An unterminated token consumes the rest of the header value, so whatever follows
			// it is not parsed as an address and nothing reports an error. An unterminated quote can
			// still yield a mailbox when an angle-addr falls inside what it absorbs, and can fold a
			// swallowed address into the display-name of the one that survives; an unterminated
			// comment leaves nothing to recover, since text inside a comment carries no meaning.
			// Unlike the ambiguity violations above, where implementations disagree about which
			// mailbox a header names, the lost address is simply gone while the surrounding headers
			// still parse and the message looks intact.
			case MimeComplianceViolation.UnbalancedQuotesInAddress:
			case MimeComplianceViolation.UnbalancedParenthesesInAddress:
				return MimeComplianceSeverity.Critical;

			case MimeComplianceViolation.ControlCharacterInAddress:
				return MimeComplianceSeverity.Major;

			// Note: Unlike an arbitrary control character, this is a deliberate legacy convention
			// rather than damage or an injection attempt, so it is rated alongside the other obsolete
			// address syntax rather than alongside ControlCharacterInAddress.
			case MimeComplianceViolation.Iso2022SequenceInLocalPart:
				return MimeComplianceSeverity.Minor;

			case MimeComplianceViolation.EmptyGroupName:
				return MimeComplianceSeverity.Minor;

			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}

		/// <summary>
		/// Get the categories of harm that a MIME compliance violation may cause.
		/// </summary>
		/// <remarks>
		/// <para>Gets what kind of harm the violation may cause, as opposed to
		/// <see cref="GetSeverity(MimeComplianceViolation)"/>, which rates how much.</para>
		/// <para>A violation may fall into more than one category, so this is a bit field.</para>
		/// </remarks>
		/// <returns>The categories.</returns>
		/// <param name="violation">The MIME compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="MimeComplianceViolation"/>.
		/// </exception>
		public static MimeComplianceCategories GetCategories (MimeComplianceViolation violation)
		{
			const MimeComplianceCategories Interop = MimeComplianceCategories.Interoperability;
			const MimeComplianceCategories DataLoss = MimeComplianceCategories.DataLoss;
			const MimeComplianceCategories Security = MimeComplianceCategories.Security;

			switch (violation) {
			// Note: A bare linefeed is the basis of SMTP smuggling. A sender and a receiver that
			// disagree about whether a bare linefeed terminates a line can be made to disagree about
			// where one message ends and the next begins.
			case MimeComplianceViolation.BareLinefeedInHeader:
			case MimeComplianceViolation.BareLinefeedInBody:
				return Interop | Security;

			// Note: A malformed header may be treated as a header by one parser and as the start of
			// the body (or as a continuation of the previous header) by another.
			case MimeComplianceViolation.InvalidHeader:
				return Interop | Security;

			// Note: A truncated header is unlikely to be interpreted differently by different
			// parsers, but whatever it was meant to say has been lost.
			case MimeComplianceViolation.IncompleteHeader:
				return Interop;

			// Note: When the Content-Type cannot be parsed, parsers fall back to different defaults,
			// which is a classic way of getting a scanner to skip content that a client will render.
			case MimeComplianceViolation.InvalidContentType:
			case MimeComplianceViolation.RepeatedContentType:
				return Interop | Security;

			// Note: As above, but an unrecognized or duplicated encoding also means the content may
			// be decoded incorrectly (or not at all), so content can be lost as well.
			case MimeComplianceViolation.InvalidContentTransferEncoding:
			case MimeComplianceViolation.RepeatedContentTransferEncoding:
				return Interop | DataLoss | Security;

			// Note: Encoding a message/rfc822 or multipart body part hides its internal structure
			// from any scanner that does not decode it, which is a well-known evasion technique.
			case MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding:
			case MimeComplianceViolation.IllegalMultipartContentTransferEncoding:
				return Interop | Security;

			// Note: An over-long line may be folded or truncated in transit, which alters the content.
			case MimeComplianceViolation.OversizedLine:
				return Interop | DataLoss;

			// Note: Without a blank line, parsers disagree about where the headers stop and the body
			// starts, so the same bytes can be read as either.
			case MimeComplianceViolation.MissingBodySeparator:
				return Interop | Security;

			// Note: Without a usable boundary, the body parts cannot be separated and are likely to
			// be presented as a single blob of text instead.
			case MimeComplianceViolation.MissingMultipartBoundaryParameter:
			case MimeComplianceViolation.MissingMultipartBoundary:
				return Interop | DataLoss;

			// Note: A boundary that needed to be repaired may be repaired differently elsewhere,
			// which changes which bytes belong to which part.
			case MimeComplianceViolation.InvalidMultipartBoundaryParameter:
				return Interop | DataLoss | Security;

			// Note: Unencoded 8-bit bytes carry no charset information, so the text is decoded by
			// guesswork and may be mangled. Parsers have coped with this for decades, so it is not
			// much of an interoperability problem in practice.
			case MimeComplianceViolation.Unexpected8BitBytesInHeader:
			case MimeComplianceViolation.Unexpected8BitBytesInBody:
				return Interop | DataLoss;

			// Note: Software written in C or C++ treats a null byte as the end of a string, so a null
			// byte can be used to hide everything after it from one program but not from another.
			case MimeComplianceViolation.UnexpectedNullBytesInHeader:
			case MimeComplianceViolation.UnexpectedNullBytesInBody:
				return Interop | Security;

			// Note: The bits in an incomplete quantum cannot be recovered.
			case MimeComplianceViolation.IncompleteBase64Quantum:
			case MimeComplianceViolation.InvalidBase64Padding:
				return DataLoss;

			// Note: Decoders disagree about whether to skip an invalid character or to stop, so they
			// can derive different content from the same bytes.
			case MimeComplianceViolation.InvalidBase64Character:
				return DataLoss | Security;

			// Note: A decoder that stops at the padding and one that keeps going will produce
			// different content, so data can be hidden after the padding.
			case MimeComplianceViolation.Base64CharactersAfterPadding:
				return DataLoss | Security;

			// Note: The letters inside an RFC 1113 comment are themselves valid base64 characters, so
			// a decoder that does not recognize the comment absorbs them as data and silently
			// corrupts everything that follows.
			case MimeComplianceViolation.ObsoleteBase64Comment:
				return DataLoss | Security;

			// Note: Malformed quoted-printable is repaired differently by different decoders.
			case MimeComplianceViolation.InvalidQuotedPrintableEncoding:
			case MimeComplianceViolation.InvalidQuotedPrintableSoftBreak:
				return DataLoss;

			// Note: The uuencode header is only used to locate the start of the encoded content and
			// to name the file. Getting it wrong does not corrupt the content itself.
			case MimeComplianceViolation.InvalidUUEncodePretext:
			case MimeComplianceViolation.InvalidUUEncodeEndMarker:
			case MimeComplianceViolation.InvalidUUEncodeFileMode:
				return Interop;

			// Note: Decoders disagree about whether to trust the length character or the line, so the
			// extra data is discarded by some and decoded as content by others.
			case MimeComplianceViolation.InvalidUUEncodedLineExtraData:
				return Interop | DataLoss;

			// Note: Content that cannot be decoded is content that is lost.
			case MimeComplianceViolation.InvalidUUEncodedContent:
			case MimeComplianceViolation.IncompleteUUEncodedContent:
				return DataLoss;

			// Note: A line whose length does not match its length character is ambiguous; decoders
			// disagree about whether to trust the character or the line.
			case MimeComplianceViolation.InvalidUUEncodedLineLength:
			case MimeComplianceViolation.IncompleteUUEncodedLine:
				return DataLoss;

			// Note: An unclosed quote, comment or group absorbs everything that follows it, so the
			// caller is handed a recipient list that is quietly shorter than the one on the wire.
			case MimeComplianceViolation.UnbalancedQuotesInAddress:
			case MimeComplianceViolation.UnbalancedParenthesesInAddress:
			case MimeComplianceViolation.MissingGroupTerminator:
				return Interop | DataLoss | Security;

			// Note: Each of these changes how many addresses a parser sees, or which mailbox one of
			// them names, so two implementations can extract genuinely different recipient lists.
			case MimeComplianceViolation.UnquotedDisplayName:
			case MimeComplianceViolation.MissingAddressSeparator:
			case MimeComplianceViolation.InvalidCharacterInDomainLiteral:
			case MimeComplianceViolation.Invalid8BitAddress:
				return Interop | DataLoss;

			// Note: The two readings of an ambiguous boundary disagree about which mailbox the
			// address names, not merely about how many there are, so a filter and a mail client can
			// be made to act on different addresses in the same header.
			case MimeComplianceViolation.AmbiguousMailboxBoundary:
				return Interop | Security;

			// Note: Nothing is lost and nothing is ambiguous to a conforming parser. The harm is
			// that software which displays the display-name in place of the address, or the group
			// name in place of its members, shows a mailbox that will not receive the reply.
			case MimeComplianceViolation.AddressInDisplayName:
			case MimeComplianceViolation.AddressInGroupDisplayName:
				return Security;

			// Note: Syntax that another implementation may read differently, or repair differently,
			// but with no reading under which an address goes missing.
			case MimeComplianceViolation.ExcessiveAngleBracketsInAddress:
			case MimeComplianceViolation.UnbalancedAngleBracketsInAddress:
			case MimeComplianceViolation.InvalidLocalPart:
			case MimeComplianceViolation.InvalidDomain:
			case MimeComplianceViolation.AddressWithoutDomain:
			case MimeComplianceViolation.ObsoleteRouteAddress:
			case MimeComplianceViolation.ExtraneousCommaInAddressList:
			case MimeComplianceViolation.ObsoleteDomainSyntax:
			case MimeComplianceViolation.TrailingDotInDomain:
			case MimeComplianceViolation.WhitespaceInDomainLiteral:
			case MimeComplianceViolation.NonConformantAddress:
				return Interop;

			case MimeComplianceViolation.NullByteInAddress:
			case MimeComplianceViolation.NullByteInDisplayName:
			case MimeComplianceViolation.LineBreakInAddress:
				return Interop | DataLoss | Security;

			case MimeComplianceViolation.ControlCharacterInAddress:
				return Interop | Security;

			case MimeComplianceViolation.Iso2022SequenceInLocalPart:
				return Interop;

			case MimeComplianceViolation.EmptyGroupName:
				return Interop;

			// Note: RFC 5322 section 3.6 limits each of these header fields to one occurrence. A
			// message that repeats one can be made to present different originators, recipients or
			// subjects to a filter than it presents to the recipient.
			case MimeComplianceViolation.RepeatedDate:
			case MimeComplianceViolation.RepeatedFrom:
			case MimeComplianceViolation.RepeatedSender:
			case MimeComplianceViolation.RepeatedReplyTo:
			case MimeComplianceViolation.RepeatedTo:
			case MimeComplianceViolation.RepeatedCc:
			case MimeComplianceViolation.RepeatedBcc:
			case MimeComplianceViolation.RepeatedMessageId:
			case MimeComplianceViolation.RepeatedSubject:
			case MimeComplianceViolation.RepeatedReturnPath:
				return Interop | Security;

			// Note: These affect only threading and the display of the resending history.
			case MimeComplianceViolation.RepeatedInReplyTo:
			case MimeComplianceViolation.RepeatedReferences:
			case MimeComplianceViolation.RepeatedResentDate:
			case MimeComplianceViolation.RepeatedResentFrom:
			case MimeComplianceViolation.RepeatedResentSender:
			case MimeComplianceViolation.RepeatedResentTo:
			case MimeComplianceViolation.RepeatedResentCc:
			case MimeComplianceViolation.RepeatedResentBcc:
			case MimeComplianceViolation.RepeatedResentMessageId:
				return Interop;

			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}

		/// <summary>
		/// Get a brief description of a MIME compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a brief, single-sentence description of what went wrong. Use
		/// <see cref="GetRemarks(MimeComplianceViolation)"/> for a longer explanation of why it matters.
		/// </remarks>
		/// <returns>The description.</returns>
		/// <param name="violation">The MIME compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="MimeComplianceViolation"/>.
		/// </exception>
		public static string GetDescription (MimeComplianceViolation violation)
		{
			switch (violation) {
			case MimeComplianceViolation.BareLinefeedInHeader:
				return "A bare linefeed character was found in a MIME part or message header.";
			case MimeComplianceViolation.BareLinefeedInBody:
				return "A bare linefeed character was found in the body of the message.";
			case MimeComplianceViolation.OversizedLine:
				return "A line was found that was longer than the SMTP limit of 1000 characters.";
			case MimeComplianceViolation.Unexpected8BitBytesInHeader:
				return "A MIME part or message header contained 8-bit bytes where only 7-bit bytes were expected.";
			case MimeComplianceViolation.Unexpected8BitBytesInBody:
				return "A MIME part's body contained 8-bit content where only 7-bit content was expected.";
			case MimeComplianceViolation.UnexpectedNullBytesInHeader:
				return "A MIME part or message header contained illegal null (0x00) bytes.";
			case MimeComplianceViolation.UnexpectedNullBytesInBody:
				return "A MIME part's body contained null (0x00) bytes without specifying a binary transfer encoding.";
			case MimeComplianceViolation.InvalidHeader:
				return "A MIME part or message header contained control (or whitespace) characters in the field name.";
			case MimeComplianceViolation.IncompleteHeader:
				return "A MIME part or message header ended prematurely at the end of the stream.";
			case MimeComplianceViolation.RepeatedContentType:
				return "A MIME part contained multiple Content-Type headers.";
			case MimeComplianceViolation.RepeatedContentTransferEncoding:
				return "A MIME part contained multiple Content-Transfer-Encoding headers.";
			case MimeComplianceViolation.RepeatedDate:
				return "The message contained more than one Date header field.";
			case MimeComplianceViolation.RepeatedFrom:
				return "The message contained more than one From header field.";
			case MimeComplianceViolation.RepeatedSender:
				return "The message contained more than one Sender header field.";
			case MimeComplianceViolation.RepeatedReplyTo:
				return "The message contained more than one Reply-To header field.";
			case MimeComplianceViolation.RepeatedTo:
				return "The message contained more than one To header field.";
			case MimeComplianceViolation.RepeatedCc:
				return "The message contained more than one Cc header field.";
			case MimeComplianceViolation.RepeatedBcc:
				return "The message contained more than one Bcc header field.";
			case MimeComplianceViolation.RepeatedMessageId:
				return "The message contained more than one Message-Id header field.";
			case MimeComplianceViolation.RepeatedInReplyTo:
				return "The message contained more than one In-Reply-To header field.";
			case MimeComplianceViolation.RepeatedReferences:
				return "The message contained more than one References header field.";
			case MimeComplianceViolation.RepeatedSubject:
				return "The message contained more than one Subject header field.";
			case MimeComplianceViolation.RepeatedReturnPath:
				return "The message contained more than one Return-Path header field.";
			case MimeComplianceViolation.RepeatedResentDate:
				return "A block of resent header fields contained more than one Resent-Date header field.";
			case MimeComplianceViolation.RepeatedResentFrom:
				return "A block of resent header fields contained more than one Resent-From header field.";
			case MimeComplianceViolation.RepeatedResentSender:
				return "A block of resent header fields contained more than one Resent-Sender header field.";
			case MimeComplianceViolation.RepeatedResentTo:
				return "A block of resent header fields contained more than one Resent-To header field.";
			case MimeComplianceViolation.RepeatedResentCc:
				return "A block of resent header fields contained more than one Resent-Cc header field.";
			case MimeComplianceViolation.RepeatedResentBcc:
				return "A block of resent header fields contained more than one Resent-Bcc header field.";
			case MimeComplianceViolation.RepeatedResentMessageId:
				return "A block of resent header fields contained more than one Resent-Message-Id header field.";
			case MimeComplianceViolation.InvalidContentType:
				return "A Content-Type header value was not valid.";
			case MimeComplianceViolation.InvalidContentTransferEncoding:
				return "A Content-Transfer-Encoding header value was not valid.";
			case MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding:
				return "A Content-Transfer-Encoding header for a message/rfc822 part contained an illegal value.";
			case MimeComplianceViolation.IllegalMultipartContentTransferEncoding:
				return "A Content-Transfer-Encoding header for a multipart contained an illegal value.";
			case MimeComplianceViolation.MissingMultipartBoundaryParameter:
				return "A boundary parameter was missing from a multipart Content-Type header.";
			case MimeComplianceViolation.InvalidMultipartBoundaryParameter:
				return "A boundary parameter in a multipart Content-Type header was not valid.";
			case MimeComplianceViolation.ExcessiveAngleBracketsInAddress:
				return "An address contained more angle brackets than the one pair that delimits an angle-addr.";
			case MimeComplianceViolation.UnbalancedAngleBracketsInAddress:
				return "An address had an opening angle bracket without a closing one, or a closing bracket without an opening one.";
			case MimeComplianceViolation.UnbalancedQuotesInAddress:
				return "An address contained a quoted-string that was never closed.";
			case MimeComplianceViolation.UnbalancedParenthesesInAddress:
				return "An address contained an unbalanced parenthesis in a comment.";
			case MimeComplianceViolation.UnquotedDisplayName:
				return "The display-name of an address contained a special character that should have been quoted.";
			case MimeComplianceViolation.AddressInDisplayName:
				return "The display-name of a mailbox was itself shaped like an address.";
			case MimeComplianceViolation.AddressInGroupDisplayName:
				return "The display-name of an address group was itself shaped like an address.";
			case MimeComplianceViolation.InvalidLocalPart:
				return "The local-part of an address was not a valid dot-atom or quoted-string.";
			case MimeComplianceViolation.MissingAddressSeparator:
				return "Two addresses in an address list were not separated by a comma.";
			case MimeComplianceViolation.AmbiguousMailboxBoundary:
				return "An address list contained two addresses whose boundary was ambiguous.";
			case MimeComplianceViolation.ExtraneousCommaInAddressList:
				return "An address list contained a comma that did not separate two addresses.";
			case MimeComplianceViolation.ObsoleteRouteAddress:
				return "An address used the obsolete source route syntax.";
			case MimeComplianceViolation.AddressWithoutDomain:
				return "An address consisted of a local-part with no domain.";
			case MimeComplianceViolation.ObsoleteDomainSyntax:
				return "The domain of an address used the obsolete syntax that allows comments and whitespace between its parts.";
			case MimeComplianceViolation.InvalidDomain:
				return "The domain of an address was not a valid dot-atom.";
			case MimeComplianceViolation.TrailingDotInDomain:
				return "The domain of an address ended with a dot.";
			case MimeComplianceViolation.WhitespaceInDomainLiteral:
				return "A domain-literal contained whitespace.";
			case MimeComplianceViolation.InvalidCharacterInDomainLiteral:
				return "A domain-literal contained a character that the domain-literal syntax does not permit.";
			case MimeComplianceViolation.Invalid8BitAddress:
				return "An address contained 8-bit bytes that were not valid UTF-8.";
			case MimeComplianceViolation.MissingGroupTerminator:
				return "An address group was not terminated with a semi-colon.";
			case MimeComplianceViolation.NonConformantAddress:
				return "An address did not conform to the address syntax defined by rfc5322.";
			case MimeComplianceViolation.NullByteInAddress:
				return "An address contained a null byte.";
			case MimeComplianceViolation.NullByteInDisplayName:
				return "The display-name of an address or group contained a null byte.";
			case MimeComplianceViolation.LineBreakInAddress:
				return "A line break appeared inside a local-part or domain.";
			case MimeComplianceViolation.ControlCharacterInAddress:
				return "An address contained a control character.";
			case MimeComplianceViolation.Iso2022SequenceInLocalPart:
				return "The local-part of an address contained an ISO-2022 shift or escape sequence.";
			case MimeComplianceViolation.EmptyGroupName:
				return "An address group had an empty name.";
			case MimeComplianceViolation.MissingBodySeparator:
				return "An empty line separating the headers from the body was missing.";
			case MimeComplianceViolation.MissingMultipartBoundary:
				return "A multipart boundary was missing.";
			case MimeComplianceViolation.IncompleteBase64Quantum:
				return "The base64 encoded content of a MIME part ended with an incomplete quantum.";
			case MimeComplianceViolation.InvalidBase64Character:
				return "The base64 encoded content of a MIME part contained invalid characters.";
			case MimeComplianceViolation.InvalidBase64Padding:
				return "The base64 encoded content of a MIME part contained invalid padding.";
			case MimeComplianceViolation.Base64CharactersAfterPadding:
				return "The base64 encoded content of a MIME part contained characters after the padding.";
			case MimeComplianceViolation.ObsoleteBase64Comment:
				return "The base64 encoded content of a MIME part contained an obsolete comment.";
			case MimeComplianceViolation.InvalidQuotedPrintableEncoding:
				return "The quoted-printable encoded content of a MIME part contained an invalid hex sequence after an '=' character.";
			case MimeComplianceViolation.InvalidQuotedPrintableSoftBreak:
				return "The quoted-printable encoded content of a MIME part contained an invalid soft-break sequence.";
			case MimeComplianceViolation.InvalidUUEncodePretext:
				return "The uuencoded content of a MIME part contained non-whitespace content before the begin marker.";
			case MimeComplianceViolation.InvalidUUEncodeFileMode:
				return "The uuencoded content of a MIME part had an invalid file mode in the begin marker.";
			case MimeComplianceViolation.InvalidUUEncodedContent:
				return "The uuencoded content of a MIME part contained invalid characters or was otherwise malformed.";
			case MimeComplianceViolation.InvalidUUEncodedLineLength:
				return "The uuencoded content of a MIME part had an invalid encoded line length.";
			case MimeComplianceViolation.IncompleteUUEncodedLine:
				return "The uuencoded content of a MIME part contained an incomplete encoded line.";
			case MimeComplianceViolation.InvalidUUEncodedLineExtraData:
				return "The uuencoded content of a MIME part had extra data beyond the end of a uuencoded line.";
			case MimeComplianceViolation.InvalidUUEncodeEndMarker:
				return "The uuencoded content of a MIME part contained non-whitespace content after the end marker.";
			case MimeComplianceViolation.IncompleteUUEncodedContent:
				return "The uuencoded content of a MIME part did not properly end.";
			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}

		/// <summary>
		/// Get a detailed explanation of a MIME compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a detailed explanation of what the relevant specifications require and the problems
		/// that the violation is likely to cause.
		/// </remarks>
		/// <returns>The remarks.</returns>
		/// <param name="violation">The MIME compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="MimeComplianceViolation"/>.
		/// </exception>
		public static string GetRemarks (MimeComplianceViolation violation)
		{
			switch (violation) {
			case MimeComplianceViolation.BareLinefeedInHeader:
				return "The Internet Message Format specification requires that all lines be terminated with a <CR><LF> sequence. Messages that deviate from this requirement may not be processed correctly by some mail software.";
			case MimeComplianceViolation.BareLinefeedInBody:
				return "The Internet Message Format specification requires that all lines be terminated with a <CR><LF> sequence. Messages that deviate from this requirement may not be processed correctly by some mail software.";
			case MimeComplianceViolation.OversizedLine:
				return "This indicates that a line was longer than the SMTP limit of 1000 characters.";
			case MimeComplianceViolation.Unexpected8BitBytesInHeader:
				return "Older Internet Message Format specifications require that headers are strictly US-ASCII while the newer Internationalized Email Headers specification allows for UTF-8. Header values that are not US-ASCII should be encoded using the encoding mechanism described in the MIME specification and/or should be valid UTF-8 as allowed in the Internationalized Email Headers specification.";
			case MimeComplianceViolation.Unexpected8BitBytesInBody:
				return "This indicates that the Content-Transfer-Encoding header for a MIME part was set to a 7-bit encoding (such as 7bit, quoted-printable, or base64) but contained non-ASCII text (or potentially even binary data).";
			case MimeComplianceViolation.UnexpectedNullBytesInHeader:
				return "Null (0x00) bytes in a message header can be used by malicious actors to prevent some MIME parsers, such as those written in languages like C or C++ which tend to use the null byte to mark the end of a buffer, from discovering content after the null byte. This technique can be used to smuggle viruses or other malicious content past content scanners.";
			case MimeComplianceViolation.UnexpectedNullBytesInBody:
				return "Null (0x00) bytes in a message body can be used by malicious actors to prevent some MIME parsers, such as those written in languages like C or C++ which tend to use the null byte to mark the end of a buffer, from discovering content after the null byte. This technique can be used to smuggle viruses or other malicious content past content scanners.";
			case MimeComplianceViolation.InvalidHeader:
				return "The Internet Message Format specification requires that all header field names be composed of printable US-ASCII characters and must not contain control characters or whitespace characters. Inclusion of these characters can lead to divergent behavior among various MIME parsers, resulting in differences in handling.";
			case MimeComplianceViolation.IncompleteHeader:
				return "This usually indicates that the message was truncated somewhere in transit and may be a sign that a MIME parser implementation earlier in transit failed to properly handle certain edge cases such as a null (0x00) byte in the message header.";
			case MimeComplianceViolation.RepeatedContentType:
				return "The MIME specifications require that each MIME part contain only one Content-Type header. Multiple Content-Type headers can lead to ambiguity and inconsistent behavior among different MIME parser implementations which may choose to use different Content-Type headers as their \"source of truth\".";
			case MimeComplianceViolation.RepeatedContentTransferEncoding:
				return "The MIME specifications require that each MIME part contain only one Content-Transfer-Encoding header. Multiple Content-Transfer-Encoding headers can lead to ambiguity and inconsistent behavior among different MIME parser implementations which may choose to use different Content-Transfer-Encoding headers as their \"source of truth\".";
			case MimeComplianceViolation.RepeatedDate:
				return "The Internet Message Format specification permits at most one Date header field. When more than one is present, agents disagree about which instance is authoritative: some take the first, some take the last. An attacker can exploit that disagreement by crafting a message where a filter or an authentication mechanism such as DKIM validates one instance while the mail client displays another.";
			case MimeComplianceViolation.RepeatedFrom:
				return "The Internet Message Format specification permits at most one From header field. When more than one is present, agents disagree about which instance is authoritative: some take the first, some take the last. An attacker can exploit that disagreement by crafting a message where a filter or an authentication mechanism such as DKIM validates one instance while the mail client displays another.";
			case MimeComplianceViolation.RepeatedSender:
				return "The Internet Message Format specification permits at most one Sender header field. When more than one is present, agents disagree about which instance is authoritative: some take the first, some take the last. An attacker can exploit that disagreement by crafting a message where a filter or an authentication mechanism such as DKIM validates one instance while the mail client displays another.";
			case MimeComplianceViolation.RepeatedReplyTo:
				return "The Internet Message Format specification permits at most one Reply-To header field. When more than one is present, agents disagree about which instance is authoritative: some take the first, some take the last. An attacker can exploit that disagreement by crafting a message where a filter or an authentication mechanism such as DKIM validates one instance while the mail client displays another.";
			case MimeComplianceViolation.RepeatedTo:
				return "The Internet Message Format specification permits at most one To header field. When more than one is present, agents disagree about which instance is authoritative: some take the first, some take the last. A message filter may therefore evaluate one instance while the mail client displays another.";
			case MimeComplianceViolation.RepeatedCc:
				return "The Internet Message Format specification permits at most one Cc header field. When more than one is present, agents disagree about which instance is authoritative: some take the first, some take the last. A message filter may therefore evaluate one instance while the mail client displays another.";
			case MimeComplianceViolation.RepeatedBcc:
				return "The Internet Message Format specification permits at most one Bcc header field. When more than one is present, agents disagree about which instance is authoritative: some take the first, some take the last. A message filter may therefore evaluate one instance while the mail client displays another.";
			case MimeComplianceViolation.RepeatedMessageId:
				return "The Internet Message Format specification permits at most one Message-Id header field. When more than one is present, agents disagree about which instance is authoritative: some take the first, some take the last. An attacker can exploit that disagreement by crafting a message where a filter or an authentication mechanism such as DKIM validates one instance while the mail client displays another.";
			case MimeComplianceViolation.RepeatedInReplyTo:
				return "The Internet Message Format specification permits at most one In-Reply-To header field. When more than one is present, agents disagree about which instance is authoritative, which may cause the message to be threaded inconsistently between mail clients.";
			case MimeComplianceViolation.RepeatedReferences:
				return "The Internet Message Format specification permits at most one References header field. When more than one is present, agents disagree about which instance is authoritative, which may cause the message to be threaded inconsistently between mail clients.";
			case MimeComplianceViolation.RepeatedSubject:
				return "The Internet Message Format specification permits at most one Subject header field. When more than one is present, agents disagree about which instance is authoritative: some take the first, some take the last. A message filter may therefore evaluate one instance while the mail client displays another.";
			case MimeComplianceViolation.RepeatedReturnPath:
				return "Legitimate messages can contain more than one Return-Path header field, but it is more often an error. All but the topmost instance should be disregarded, because the topmost was added nearest to the mailbox that received the message.";
			case MimeComplianceViolation.RepeatedResentDate:
				return "The Internet Message Format specification permits at most one Resent-Date header field per block of resent header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the message. Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly informational and must not be used when processing replies, so the practical consequences are limited to how the resending history is displayed.";
			case MimeComplianceViolation.RepeatedResentFrom:
				return "The Internet Message Format specification permits at most one Resent-From header field per block of resent header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the message. Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly informational and must not be used when processing replies, so the practical consequences are limited to how the resending history is displayed.";
			case MimeComplianceViolation.RepeatedResentSender:
				return "The Internet Message Format specification permits at most one Resent-Sender header field per block of resent header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the message. Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly informational and must not be used when processing replies, so the practical consequences are limited to how the resending history is displayed.";
			case MimeComplianceViolation.RepeatedResentTo:
				return "The Internet Message Format specification permits at most one Resent-To header field per block of resent header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the message. Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly informational and must not be used when processing replies, so the practical consequences are limited to how the resending history is displayed.";
			case MimeComplianceViolation.RepeatedResentCc:
				return "The Internet Message Format specification permits at most one Resent-Cc header field per block of resent header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the message. Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly informational and must not be used when processing replies, so the practical consequences are limited to how the resending history is displayed.";
			case MimeComplianceViolation.RepeatedResentBcc:
				return "The Internet Message Format specification permits at most one Resent-Bcc header field per block of resent header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the message. Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly informational and must not be used when processing replies, so the practical consequences are limited to how the resending history is displayed.";
			case MimeComplianceViolation.RepeatedResentMessageId:
				return "The Internet Message Format specification permits at most one Resent-Message-Id header field per block of resent header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the message. Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly informational and must not be used when processing replies, so the practical consequences are limited to how the resending history is displayed.";
			case MimeComplianceViolation.InvalidContentType:
				return "This indicates that the Content-Type header was not properly formatted and could not be parsed. Since MIME parsers rely on the Content-Type header to decide how to interpret the content of a MIME part, an invalid Content-Type header can lead to ambiguity and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidContentTransferEncoding:
				return "This indicates that the Content-Transfer-Encoding header did not contain a valid value and could not be parsed.";
			case MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding:
				return "The MIME specifications do not allow message/rfc822 Content-Transfer-Encoding headers to specify any encoding that transforms the content in any way (such as quoted-printable or base64).";
			case MimeComplianceViolation.IllegalMultipartContentTransferEncoding:
				return "The MIME specifications do not allow multipart Content-Transfer-Encoding headers to specify any encoding that transforms the content in any way (such as quoted-printable or base64).";
			case MimeComplianceViolation.MissingMultipartBoundaryParameter:
				return "The MIME specifications require that each multipart Content-Type header include a boundary parameter. A multipart that does not define a boundary can lead to ambiguity and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidMultipartBoundaryParameter:
				return "A boundary parameter in a multipart Content-Type header must be a valid boundary string as defined by the MIME specifications. Invalid boundary parameters can lead to ambiguity and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.ExcessiveAngleBracketsInAddress:
				return "Section 7.1.2 of rfc7103 describes address values such as \"<<user@example.com>>\" and notes that they can safely be interpreted as the same address with a single pair of brackets. Unlike an unbalanced bracket, a repeated one leaves no doubt about where the address begins and ends, so implementations that discard the extras all arrive at the same mailbox. It is still a departure from the angle-addr production, and usually indicates a mailer that has wrapped an address which was already wrapped.";
			case MimeComplianceViolation.UnbalancedAngleBracketsInAddress:
				return "Section 7.1.3 of rfc7103 describes address values such as \"Name <user@example.com\" and \"user@example.org>\". Recovering from an unbalanced bracket requires guessing where the address was meant to end, and parsers that guess differently will extract different addresses.";
			case MimeComplianceViolation.UnbalancedQuotesInAddress:
				return "Section 7.1.6 of rfc7103 describes address values such as \"\\\"Unterminated <user@example.com>\". An unclosed quote absorbs everything that follows it. Whether an address survives depends on what gets absorbed: a quote followed by an angle-addr still yields that mailbox, but one followed only by an addr-spec yields nothing at all. No error is raised either way, so a header that contributes fewer recipients than its author wrote is indistinguishable from one that parsed cleanly.";
			case MimeComplianceViolation.UnbalancedParenthesesInAddress:
				return "Section 7.1.4 of rfc7103 describes address values such as \"Name (unbalanced <user@example.com>\". As with an unclosed quote, an unclosed comment consumes the remainder of the header value, so any addresses that follow it are lost rather than merely misparsed: text inside a comment carries no meaning, so there is nothing left for a parser to recover. No error is raised, so the message appears intact while carrying fewer recipients than its author wrote.";
			case MimeComplianceViolation.UnquotedDisplayName:
				return "An unquoted display-name may only contain atoms, so values such as \"Doe, John <jdoe@example.com>\" and \"user@example.com <user@example.com>\" are not valid. The comma case is the most damaging, because a parser that does not special-case it will split the one address into two.";
			case MimeComplianceViolation.AddressInDisplayName:
				return "A display-name such as the one in \"\\\"admin@example.com\\\" <attacker@example.org>\" is legal, but software that shows the display-name in place of the address will present a mailbox that will not receive the reply. The address that rfc5322 defines as authoritative is the one inside the angle brackets.";
			case MimeComplianceViolation.AddressInGroupDisplayName:
				return "A group name such as the one in \"\\\"admin@example.com\\\": attacker@example.org;\" is legal, but software that shows the group name in place of its members will present a mailbox that is not in the group. A group name is a label, not a recipient.";
			case MimeComplianceViolation.InvalidLocalPart:
				return "A dot-atom may not contain two consecutive dots or end with a dot, so local-parts such as \"first..last\" and \"first.\" are not valid. This is also reported when an unquoted special appears inside the local-part, as in \"a[b@example.com\" or \"a b@example.com\": section 3.4.1 of rfc5322 admits such characters only inside a quoted-string, so the local-part ends at the offending character and the rest of the address is left with no production that can consume it. Receiving systems differ over whether to reject such an address, strip the offending characters, or pass the local-part through verbatim.";
			case MimeComplianceViolation.MissingAddressSeparator:
				return "Section 7.1.5 of rfc7103 describes address lists such as \"a@example.com b@example.com\". A parser must guess whether this is two addresses or one address with a malformed display-name, and the two readings produce different sets of recipients.";
			case MimeComplianceViolation.AmbiguousMailboxBoundary:
				return "A list such as \"<attacker@example.org> <admin@example.com>\" contains an angle-addr on at least one side of a missing separator, so a parser that recovers by treating the leading text as a display-name will read a single mailbox where a parser that recovers by splitting will read two. Unlike an ordinary missing comma, the two readings do not merely differ in how many addresses they produce, they disagree about which mailbox the address belongs to.";
			case MimeComplianceViolation.ExtraneousCommaInAddressList:
				return "Section 7.1.5 of rfc7103 describes address lists such as \"a@example.com,,,b@example.com\", as well as lists with leading or trailing commas. The empty entries are not addresses and are typically ignored, but their presence usually indicates that the generating software dropped an address it intended to include.";
			case MimeComplianceViolation.ObsoleteRouteAddress:
				return "The obs-route syntax described in section 4.4 of rfc5322, as in \"<@a.example,@b.example:user@example.com>\", must not be generated by conforming software. The route itself is meant to be ignored, but software that does not recognize the syntax may mistake the first domain in the route for the address domain.";
			case MimeComplianceViolation.AddressWithoutDomain:
				return "Section 7.1.7 of rfc7103 describes \"naked\" local-parts such as \"username\". Such an address is only meaningful relative to some implied domain, so different systems will complete it differently, or not at all.";
			case MimeComplianceViolation.ObsoleteDomainSyntax:
				return "The obs-domain syntax described in section 4.4 of rfc5322 allows folding whitespace and comments around the dots of a domain, as in \"user@example (comment) .com\". A conforming domain is a single dot-atom, so software that does not implement the obsolete grammar will read a different domain than software that does.";
			case MimeComplianceViolation.InvalidDomain:
				return "A dot-atom may not contain two consecutive dots or begin with a dot, so domains such as \"example..com\" and \".example.com\" are not valid. A domain that merely ends with a dot is reported as TrailingDotInDomain instead, because a trailing dot is the fully qualified form of a domain name in the DNS and is far more likely to be deliberate. Receiving systems differ over whether to reject such an address, collapse the empty labels, or pass the domain through verbatim.";
			case MimeComplianceViolation.TrailingDotInDomain:
				return "A trailing dot, as in \"user@example.com.\", denotes a fully qualified domain in the DNS but is not part of the domain grammar in rfc5322. Parsers that strip it and parsers that retain it will disagree about whether two otherwise identical addresses are equal.";
			case MimeComplianceViolation.WhitespaceInDomainLiteral:
				return "The dtext rule in section 3.4.1 of rfc5322 does not permit whitespace inside the brackets of a domain-literal, as in \"user@[ 127.0.0.1 ]\". Parsers that strip the whitespace and parsers that preserve or reject it will not agree on the address.";
			case MimeComplianceViolation.InvalidCharacterInDomainLiteral:
				return "The dtext rule in section 3.4.1 of rfc5322 excludes '[', ']' and '\\' from the contents of a domain-literal, as in \"user@[10.0.0.1[]\". Because ']' is the only thing that can end a domain-literal, an unescapable bracket leaves no way to tell where the author intended the address to end, and the domain cannot be recovered from what was written. Control characters and invalid 8-bit bytes inside a domain-literal are reported as ControlCharacterInAddress and Invalid8BitAddress instead, and rfc6532 adds the remaining 8-bit bytes to dtext.";
			case MimeComplianceViolation.Invalid8BitAddress:
				return "The internationalized address syntax in rfc6532 extends the address grammar to UTF-8 and to nothing else, so 8-bit bytes that are not valid UTF-8 have no defined interpretation. A parser that falls back to a single-byte charset will produce a different address than one that rejects the header, which may result in mail being delivered to the wrong mailbox.";
			case MimeComplianceViolation.MissingGroupTerminator:
				return "The group syntax in section 3.4 of rfc5322 requires a terminating ';', as in \"Friends: a@example.com;\". Without it, a parser must guess where the group ends, and addresses that follow the group may be absorbed into it.";
			case MimeComplianceViolation.NonConformantAddress:
				return "This is the general case, used when an address departs from the grammar in a way that none of the more specific violations describes. Receiving systems differ widely in how much malformed syntax they will accept and in how they repair what they accept, so an address that only some implementations can read may resolve to different mailboxes, or to none at all, depending on which software handles the message.";
			case MimeComplianceViolation.NullByteInAddress:
				return "A null byte is not permitted anywhere in a header, but inside an address it is more dangerous than elsewhere. Software written in or interfacing with C treats a null as a string terminator, so an address such as \"us<NUL>er@example.com\" may be read as the complete address \"us\" by one component and as \"user@example.com\" by another. That disagreement is the point of the construct: a filter, an audit log and the delivering agent can each be made to see a different mailbox from the same header. This is reported in addition to UnexpectedNullBytesInHeader, which identifies only the line that the null byte appeared on.";
							case MimeComplianceViolation.NullByteInDisplayName:
								return "This is reported instead of NullByteInAddress when the null falls in a display-name rather than in an addr-spec, because the two call for different handling: a display-name is presentation and can be discarded without affecting delivery, whereas an addr-spec cannot. The danger is correspondingly different rather than smaller. The mailbox is unambiguous, but a client that stops at the null shows a different name than one that does not, so \"Bank<NUL>evil\" can be made to read as \"Bank\" in the message list and as something else in a filter or an audit log. This is reported in addition to UnexpectedNullBytesInHeader, which identifies only the line that the null byte appeared on.";
							case MimeComplianceViolation.LineBreakInAddress:
								return "Folding whitespace is permitted around the tokens of an address, but the dot-atom-text production in section 3.2.3 of rfc5322 admits none inside a local-part or domain, so a line break within one of those tokens cannot be produced by a conforming mailer. It is most often seen when an application has concatenated unvalidated input into a header, which is the header injection technique described in section 5 of rfc5321: the attacker supplies a line break in the hope that some component in the chain will treat what follows as a new header or a new command. Even where that fails, implementations differ on whether to unfold, reject or truncate the address, so the recipient that is finally used may not be the one an auditor sees. A line break inside a quoted local-part is also reported, even though section 3.2.4 of rfc5322 permits folding whitespace inside a quoted-string, because a quoted local-part is rare enough that implementations mishandle it in practice.";
			case MimeComplianceViolation.ControlCharacterInAddress:
				return "The atom, quoted-string and domain-literal productions in rfc5322 are all built from printable characters and whitespace, so a control character such as ESC or DEL can only have been introduced deliberately or by a mangled encoding. Control characters are stripped by some implementations and preserved by others, so the address may name a different mailbox depending on which software resolves it, and an escape sequence that survives into a log or a terminal-based mail client may be interpreted there rather than displayed.";
			case MimeComplianceViolation.Iso2022SequenceInLocalPart:
				return "ISO-2022-JP and its relatives switch between character sets using escape sequences such as \"ESC $ B\", and, in the Korean and Chinese variants, using the shift-out and shift-in control characters. Japanese mailers have historically used these inside the local-part of an addr-spec in order to carry Japanese text in a mailbox name, a practice that predates and is entirely separate from the internationalized address syntax defined by rfc6532. This is reported separately from ControlCharacterInAddress because the two call for different handling: an arbitrary control character in an address is either damage or an injection attempt, whereas a well-formed ISO-2022 sequence in a local-part is a deliberate legacy convention that a receiving system may wish to decode rather than strip. It remains a violation either way, because implementations that strip the escapes and implementations that preserve them will not agree on which mailbox the address names. The construct is particularly awkward inside a quoted local-part, because the second byte of a JIS X 0208 pair may be a backslash or a double-quote, which then has to be written as a quoted-pair in order to survive the quoted-string grammar.";
			case MimeComplianceViolation.EmptyGroupName:
				return "The group syntax in section 3.4 of rfc5322 is display-name \":\" [group-list] \";\", and a display-name is a phrase, which requires at least one word. A group introduced by a bare colon therefore has no name for a client to display, and parsers disagree over whether to treat the colon as introducing a group at all or as a stray character in an ordinary address.";
			case MimeComplianceViolation.MissingBodySeparator:
				return "The Internet Message Format specifications require that an empty line separate the headers from the body of a message. This empty line serves as a clear delimiter between the headers and the body, allowing MIME parsers to correctly identify where the headers end and the body begins. A missing body separator can lead to ambiguity when parsing the message.";
			case MimeComplianceViolation.MissingMultipartBoundary:
				return "When a multipart does not contain any boundary markers within its content, it can lead to ambiguity and inconsistent behavior among different MIME parser implementations which may opt to treat the content as a single part rather than a multipart message.";
			case MimeComplianceViolation.IncompleteBase64Quantum:
				return "The MIME specifications require base64 encoded content be a multiple of 4 bytes (a \"quantum\") in length. An incomplete quantum at the end of the content suggests that the base64 encoded content was either truncated or otherwise corrupted and can therefore lead to inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidBase64Character:
				return "Invalid characters within base64 content can lead to decoding issues and inconsistent behavior among different MIME parser implementations which may stop decoding as soon as this scenario is encountered while others may ignore these characters and continue decoding.";
			case MimeComplianceViolation.InvalidBase64Padding:
				return "Invalid padding within base64 content can lead to decoding issues and inconsistent behavior among different MIME parser implementations. Some base64 decoders will ignore extraneous '=' padding characters if any are found within the middle of the base64 encoded block while others will treat decode it as 6 bits of 0's and may stop decoding as soon as they are encountered.";
			case MimeComplianceViolation.Base64CharactersAfterPadding:
				return "Base64 characters found after padding ('=') in a base64 encoded block are not allowed by the MIME specifications and can lead to inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.ObsoleteBase64Comment:
				return "RFC 1113 (a Privacy Enhanced Mail specification) allowed for comments delimited by the '*' character in what later became known as \"base64 encoding\". This was obsoleted in RFC 1421 (which replaced RFC 1113) and RFC 1341 (the first MIME specification) explicitly disallowed it, but some mailers may generate such content. Since the vast majority of MIME base64 decoders do not support comments in base64 content, the presence of such comments can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidQuotedPrintableEncoding:
				return "Incorrect hex-encoded sequences in quoted-printable content can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidQuotedPrintableSoftBreak:
				return "A soft line break in quoted-printable content is represented by an equal sign (=) character followed immediately by a <CR><LF> sequence. This error indicates that an equal sign was immediately followed by an incomplete <CR><LF> sequence which can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidUUEncodePretext:
				return "UUEncoding requires that only lines containing whitespace are allowed before the begin marker. Non-whitespace content before the begin marker can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidUUEncodeFileMode:
				return "The UUEncoding begin marker should contain a file mode that is 3-4 digits long. An invalid file mode can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidUUEncodedContent:
				return "Incorrect line lengths and/or invalid characters in uuencoded content can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidUUEncodedLineLength:
				return "Each line in UUEncoding has a specific length encoded in the first byte of the line. This length must be between 0 and 45 (inclusive) and is used to determine how many bytes of data are represented by the line. An invalid line length can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.IncompleteUUEncodedLine:
				return "Each line in UUEncoding has a specific length encoded in the first byte of the line. Incomplete lines can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidUUEncodedLineExtraData:
				return "Each line in UUEncoding has a specific length encoded in the first byte of the line. Extra data beyond the end of the uuencoded line can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidUUEncodeEndMarker:
				return "UUEncoding requires that only whitespace is allowed after the end marker. Non-whitespace content after the end marker can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.IncompleteUUEncodedContent:
				return "UUEncoding requires that the encoded content is properly terminated with an end marker. Missing or malformed end markers can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}
	}
}
