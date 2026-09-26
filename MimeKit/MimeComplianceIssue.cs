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
	/// </remarks>
	public readonly struct MimeComplianceIssue
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MimeComplianceIssue"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeComplianceIssue"/> without any column information.
		/// </remarks>
		/// <param name="violation">The specific MIME compliance violation that occurred.</param>
		/// <param name="streamOffset">The offset within the stream where the violation was found.</param>
		/// <param name="lineNumber">The one-based line number where the violation was found.</param>
		public MimeComplianceIssue (MimeComplianceViolation violation, long streamOffset, int lineNumber) : this (violation, streamOffset, lineNumber, 0)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MimeComplianceIssue"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MimeComplianceIssue"/>.
		/// </remarks>
		/// <param name="violation">The specific MIME compliance violation that occurred.</param>
		/// <param name="streamOffset">The offset within the stream where the violation was found.</param>
		/// <param name="lineNumber">The one-based line number where the violation was found.</param>
		/// <param name="columnNumber">The one-based column number where the violation was found, or <c>0</c> if unknown.</param>
		public MimeComplianceIssue (MimeComplianceViolation violation, long streamOffset, int lineNumber, int columnNumber)
		{
			Violation = violation;
			StreamOffset = streamOffset;
			LineNumber = lineNumber;
			ColumnNumber = columnNumber;
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
		/// Get the severity of the MIME compliance violation.
		/// </summary>
		/// <remarks>
		/// <para>Gets how much practical harm the violation is likely to cause, assuming the message
		/// is being transmitted over the network.</para>
		/// <para>Use <see cref="GetSeverity(MimeComplianceContext)"/> instead if the message was read
		/// from a local message store, where a few violations are routine and harmless.</para>
		/// </remarks>
		/// <value>The severity.</value>
		public MimeComplianceSeverity Severity {
			get { return GetSeverity (Violation, MimeComplianceContext.Transport); }
		}

		/// <summary>
		/// Get the severity of the MIME compliance violation in a particular context.
		/// </summary>
		/// <remarks>
		/// Gets how much practical harm the violation is likely to cause when the message is used in
		/// the specified context. Use this instead of <see cref="Severity"/> when the message came
		/// from a local message store rather than from the network.
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
			case MimeComplianceViolation.InvalidWrapping:
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
			case MimeComplianceViolation.MultipleContentTypes:
			case MimeComplianceViolation.MultipleContentTransferEncodings:
			case MimeComplianceViolation.UnexpectedNullBytesInHeader:
			case MimeComplianceViolation.UnexpectedNullBytesInBody:
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
				return MimeComplianceSeverity.Major;

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
			const MimeComplianceCategories Cosmetic = MimeComplianceCategories.Cosmetic;
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
			case MimeComplianceViolation.MultipleContentTypes:
				return Interop | Security;

			// Note: As above, but an unrecognized or duplicated encoding also means the content may
			// be decoded incorrectly (or not at all), so content can be lost as well.
			case MimeComplianceViolation.InvalidContentTransferEncoding:
			case MimeComplianceViolation.MultipleContentTransferEncodings:
				return Interop | DataLoss | Security;

			// Note: Encoding a message/rfc822 or multipart body part hides its internal structure
			// from any scanner that does not decode it, which is a well-known evasion technique.
			case MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding:
			case MimeComplianceViolation.IllegalMultipartContentTransferEncoding:
				return Interop | Security;

			// Note: An over-long line may be folded or truncated in transit, which alters the content.
			case MimeComplianceViolation.InvalidWrapping:
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
				return Interop;

			// Note: The file mode is advisory and is ignored by most software.
			case MimeComplianceViolation.InvalidUUEncodeFileMode:
			case MimeComplianceViolation.InvalidUUEncodedLineExtraData:
				return Cosmetic;

			// Note: Content that cannot be decoded is content that is lost.
			case MimeComplianceViolation.InvalidUUEncodedContent:
			case MimeComplianceViolation.IncompleteUUEncodedContent:
				return DataLoss;

			// Note: A line whose length does not match its length character is ambiguous; decoders
			// disagree about whether to trust the character or the line.
			case MimeComplianceViolation.InvalidUUEncodedLineLength:
			case MimeComplianceViolation.IncompleteUUEncodedLine:
				return DataLoss;

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
			case MimeComplianceViolation.InvalidHeader:
				return "A MIME part or message header contained control (or whitespace) characters in the field name.";
			case MimeComplianceViolation.IncompleteHeader:
				return "A MIME part or message header ended prematurely at the end of the stream.";
			case MimeComplianceViolation.InvalidContentType:
				return "A Content-Type header value was not valid.";
			case MimeComplianceViolation.MultipleContentTypes:
				return "A MIME part contained multiple Content-Type headers.";
			case MimeComplianceViolation.InvalidContentTransferEncoding:
				return "A Content-Transfer-Encoding header value was not valid.";
			case MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding:
				return "A Content-Transfer-Encoding header for a message/rfc822 part contained an illegal value.";
			case MimeComplianceViolation.IllegalMultipartContentTransferEncoding:
				return "A Content-Transfer-Encoding header for a multipart contained an illegal value.";
			case MimeComplianceViolation.MultipleContentTransferEncodings:
				return "A MIME part contained multiple Content-Transfer-Encoding headers.";
			case MimeComplianceViolation.InvalidWrapping:
				return "A line was found that was longer than the SMTP limit of 1000 characters.";
			case MimeComplianceViolation.MissingBodySeparator:
				return "An empty line separating the headers from the body was missing.";
			case MimeComplianceViolation.MissingMultipartBoundaryParameter:
				return "A boundary parameter was missing from a multipart Content-Type header.";
			case MimeComplianceViolation.InvalidMultipartBoundaryParameter:
				return "A boundary parameter in a multipart Content-Type header was not valid.";
			case MimeComplianceViolation.MissingMultipartBoundary:
				return "A multipart boundary was missing.";
			case MimeComplianceViolation.Unexpected8BitBytesInHeader:
				return "A MIME part or message header contained 8-bit bytes where only 7-bit bytes were expected.";
			case MimeComplianceViolation.Unexpected8BitBytesInBody:
				return "A MIME part's body contained 8-bit content where only 7-bit content was expected.";
			case MimeComplianceViolation.UnexpectedNullBytesInHeader:
				return "A MIME part or message header contained illegal null (0x00) bytes.";
			case MimeComplianceViolation.UnexpectedNullBytesInBody:
				return "A MIME part's body contained null (0x00) bytes without specifying a binary transfer encoding.";
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
			case MimeComplianceViolation.InvalidHeader:
				return "The Internet Message Format specification requires that all header field names be composed of printable US-ASCII characters and must not contain control characters or whitespace characters. Inclusion of these characters can lead to divergent behavior among various MIME parsers, resulting in differences in handling.";
			case MimeComplianceViolation.IncompleteHeader:
				return "This usually indicates that the message was truncated somewhere in transit and may be a sign that a MIME parser implementation earlier in transit failed to properly handle certain edge cases such as a null (0x00) byte in the message header.";
			case MimeComplianceViolation.InvalidContentType:
				return "This indicates that the Content-Type header was not properly formatted and could not be parsed. Since MIME parsers rely on the Content-Type header to decide how to interpret the content of a MIME part, an invalid Content-Type header can lead to ambiguity and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.MultipleContentTypes:
				return "The MIME specifications require that each MIME part contain only one Content-Type header. Multiple Content-Type headers can lead to ambiguity and inconsistent behavior among different MIME parser implementations which may choose to use different Content-Type headers as their \"source of truth\".";
			case MimeComplianceViolation.InvalidContentTransferEncoding:
				return "This indicates that the Content-Transfer-Encoding header did not contain a valid value and could not be parsed.";
			case MimeComplianceViolation.IllegalMessageRfc822ContentTransferEncoding:
				return "The MIME specifications do not allow message/rfc822 Content-Transfer-Encoding headers to specify any encoding that transforms the content in any way (such as quoted-printable or base64).";
			case MimeComplianceViolation.IllegalMultipartContentTransferEncoding:
				return "The MIME specifications do not allow multipart Content-Transfer-Encoding headers to specify any encoding that transforms the content in any way (such as quoted-printable or base64).";
			case MimeComplianceViolation.MultipleContentTransferEncodings:
				return "The MIME specifications require that each MIME part contain only one Content-Transfer-Encoding header. Multiple Content-Transfer-Encoding headers can lead to ambiguity and inconsistent behavior among different MIME parser implementations which may choose to use different Content-Transfer-Encoding headers as their \"source of truth\".";
			case MimeComplianceViolation.InvalidWrapping:
				return "This indicates that a line was longer than the SMTP limit of 1000 characters.";
			case MimeComplianceViolation.MissingBodySeparator:
				return "The Internet Message Format specifications require that an empty line separate the headers from the body of a message. This empty line serves as a clear delimiter between the headers and the body, allowing MIME parsers to correctly identify where the headers end and the body begins. A missing body separator can lead to ambiguity when parsing the message.";
			case MimeComplianceViolation.MissingMultipartBoundaryParameter:
				return "The MIME specifications require that each multipart Content-Type header include a boundary parameter. A multipart that does not define a boundary can lead to ambiguity and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidMultipartBoundaryParameter:
				return "A boundary parameter in a multipart Content-Type header must be a valid boundary string as defined by the MIME specifications. Invalid boundary parameters can lead to ambiguity and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.MissingMultipartBoundary:
				return "When a multipart does not contain any boundary markers within its content, it can lead to ambiguity and inconsistent behavior among different MIME parser implementations which may opt to treat the content as a single part rather than a multipart message.";
			case MimeComplianceViolation.Unexpected8BitBytesInHeader:
				return "Older Internet Message Format specifications require that headers are strictly US-ASCII while the newer Internationalized Email Headers specification allows for UTF-8. Header values that are not US-ASCII should be encoded using the encoding mechanism described in the MIME specification and/or should be valid UTF-8 as allowed in the Internationalized Email Headers specification.";
			case MimeComplianceViolation.Unexpected8BitBytesInBody:
				return "This indicates that the Content-Transfer-Encoding header for a MIME part was set to a 7-bit encoding (such as 7bit, quoted-printable, or base64) but contained non-ASCII text (or potentially even binary data).";
			case MimeComplianceViolation.UnexpectedNullBytesInHeader:
				return "Null (0x00) bytes in a message header can be used by malicious actors to prevent some MIME parsers, such as those written in languages like C or C++ which tend to use the null byte to mark the end of a buffer, from discovering content after the null byte. This technique can be used to smuggle viruses or other malicious content past content scanners.";
			case MimeComplianceViolation.UnexpectedNullBytesInBody:
				return "Null (0x00) bytes in a message body can be used by malicious actors to prevent some MIME parsers, such as those written in languages like C or C++ which tend to use the null byte to mark the end of a buffer, from discovering content after the null byte. This technique can be used to smuggle viruses or other malicious content past content scanners.";
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
