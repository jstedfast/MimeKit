//
// MimeComplianceViolation.cs
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

namespace MimeKit {
	/// <summary>
	/// An enumeration of potential MIME compliance violations.
	/// </summary>
	/// <remarks>
	/// <para>An enumeration of the ways in which a message may deviate from the Internet Message
	/// Format and MIME specifications, as reported to an <see cref="IMimeComplianceLogger"/> by
	/// <see cref="MimeReader"/>.</para>
	/// <note type="note">The <c>UUEncode</c> and <c>UUEncoded</c> spellings are deliberately
	/// distinct: <c>UUEncode</c> names a construct belonging to the uuencode format itself (the
	/// pretext, the file mode within the begin marker, the end marker) while <c>UUEncoded</c>
	/// names the encoded data.</note>
	/// </remarks>
	public enum MimeComplianceViolation
	{
		// Note: The members below are grouped by subject matter so that the enumeration reads well,
		// but the numeric values are append-only. C# inlines enum constants into consuming
		// assemblies at compile time, so renumbering an existing member silently changes the meaning
		// of code that was compiled against an earlier version of MimeKit, as well as any value that
		// has been persisted or logged. A new violation therefore belongs in whichever #region
		// covers its subject matter, but it must take the next unused value rather than the value
		// that its position would suggest. MimeComplianceViolationTests enforces both halves of this
		// rule.

		/// <summary>
		/// No violation.
		/// </summary>
		/// <remarks>
		/// This value is never reported by <see cref="MimeReader"/>. It exists so that the default value of
		/// a <see cref="MimeComplianceIssue"/> is distinguishable from a genuine violation.
		/// </remarks>
		None                                        = 0,

		#region Line Format

		/// <summary>
		/// A bare linefeed character was found in a MIME part or message header.
		/// </summary>
		/// <remarks>
		/// <para>The Internet Message Format specification requires that all lines be terminated with
		/// a &lt;CR&gt;&lt;LF&gt; sequence. Messages that deviate from this requirement may not be
		/// processed correctly by some mail software.</para>
		/// <note type="note">This is generally acceptable when parsing messages from disk storage on
		/// UNIX systems but should not occur when transmitting messages over the network via protocols
		/// such as SMTP, POP3 or IMAP.</note>
		/// </remarks>
		BareLinefeedInHeader                        = 1,

		/// <summary>
		/// A bare linefeed character was found in the body of the message.
		/// </summary>
		/// <remarks>
		/// <para>The Internet Message Format specification requires that all lines be terminated with
		/// a &lt;CR&gt;&lt;LF&gt; sequence. Messages that deviate from this requirement may not be
		/// processed correctly by some mail software.</para>
		/// <note type="note">This is generally acceptable when parsing messages from disk storage on
		/// UNIX systems but should not occur when transmitting messages over the network via protocols
		/// such as SMTP, POP3 or IMAP.</note>
		/// </remarks>
		BareLinefeedInBody                          = 2,

		/// <summary>
		/// A line was found that exceeded the maximum line length permitted by the Internet Message Format.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification limits a line to 998 characters plus the
		/// &lt;CR&gt;&lt;LF&gt; terminator. Some handling agents will not examine content beyond the first
		/// 998 bytes of a line, which provides an opportunity to hide malicious content.
		/// </remarks>
		OversizedLine                               = 3,

		/// <summary>
		/// A MIME part or message header contained 8-bit bytes where only 7-bit bytes were expected.
		/// </summary>
		/// <remarks>
		/// <para>Older Internet Message Format specifications require that headers are strictly US-ASCII
		/// while the newer Internationalized Email Headers specification allows for UTF-8. Header values
		/// that are not US-ASCII should be encoded using the encoding mechanism described in the MIME
		/// specification and/or should be valid UTF-8 as allowed in the Internationalized Email Headers
		/// specification.</para>
		/// <note type="note">This violation will only be raised if the 8-bit text in the header value is
		/// not valid UTF-8.</note>
		/// </remarks>
		Unexpected8BitBytesInHeader                 = 4,

		/// <summary>
		/// A MIME part's body contained 8-bit content where only 7-bit content was expected.
		/// </summary>
		/// <remarks>
		/// This indicates that the Content-Transfer-Encoding header for a MIME part was set to a 7-bit
		/// encoding (such as <c>7bit</c>, <c>quoted-printable</c>, or <c>base64</c>) but contained
		/// non-ASCII text (or potentially even binary data).
		/// </remarks>
		Unexpected8BitBytesInBody                   = 5,

		/// <summary>
		/// A MIME part or message header contained illegal null (<c>0x00</c>) bytes.
		/// </summary>
		/// <remarks>
		/// Null (<c>0x00</c>) bytes in a message header can be used by malicious actors to prevent some
		/// MIME parsers, such as those written in languages like C or C++ which tend to use the null byte
		/// to mark the end of a buffer, from discovering content after the null byte. This technique can
		/// be used to smuggle viruses or other malicious content past content scanners.
		/// </remarks>
		UnexpectedNullBytesInHeader                 = 6,

		/// <summary>
		/// A MIME part's body contained null (<c>0x00</c>) bytes without specifying a binary transfer encoding.
		/// </summary>
		/// <remarks>
		/// Null (<c>0x00</c>) bytes in a message body can be used by malicious actors to prevent some
		/// MIME parsers, such as those written in languages like C or C++ which tend to use the null byte
		/// to mark the end of a buffer, from discovering content after the null byte. This technique can
		/// be used to smuggle viruses or other malicious content past content scanners.
		/// </remarks>
		UnexpectedNullBytesInBody                   = 7,

		#endregion

		#region Header Syntax

		/// <summary>
		/// A MIME part or message header contained control (or whitespace) characters in the field name.
		/// </summary>
		/// <remarks>
		/// <para>The Internet Message Format specification requires that all header field names be composed
		/// of printable US-ASCII characters and must not contain control characters or whitespace characters.
		/// Inclusion of these characters can lead to divergent behavior among various MIME parsers,
		/// resulting in differences in handling.</para>
		/// <note type="note">The Internet Message Format specification allows for whitespace characters to
		/// exist between the end of the field name and the <c>':'</c> character that delineates the header
		/// name and value. In that particular case, the <see cref="InvalidHeader"/> violation will NOT be
		/// raised.</note>
		/// </remarks>
		InvalidHeader                               = 8,

		/// <summary>
		/// A MIME part or message header ended prematurely at the end of the stream.
		/// </summary>
		/// <remarks>
		/// This usually indicates that the message was truncated somewhere in transit and may be a sign that
		/// a MIME parser implementation earlier in transit failed to properly handle certain edge cases such
		/// as a null (<c>0x00</c>) byte in the message header.
		/// </remarks>
		IncompleteHeader                            = 9,

		#endregion

		#region Repeated Header Fields

		/// <summary>
		/// A MIME part contained multiple Content-Type headers.
		/// </summary>
		/// <remarks>
		/// The MIME specifications require that each MIME part contain only one Content-Type header.
		/// Multiple Content-Type headers can lead to ambiguity and inconsistent behavior among different
		/// MIME parser implementations which may choose to use different Content-Type headers as their
		/// "source of truth".
		/// </remarks>
		RepeatedContentType                         = 10,

		/// <summary>
		/// A MIME part contained multiple Content-Transfer-Encoding headers.
		/// </summary>
		/// <remarks>
		/// The MIME specifications require that each MIME part contain only one Content-Transfer-Encoding header.
		/// Multiple Content-Transfer-Encoding headers can lead to ambiguity and inconsistent behavior among different
		/// MIME parser implementations which may choose to use different Content-Transfer-Encoding headers as their
		/// "source of truth".
		/// </remarks>
		RepeatedContentTransferEncoding             = 11,

		/// <summary>
		/// The message contained more than one Date header field.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification permits at most one Date header field. When more than one is present,
		/// agents disagree about which instance is authoritative: some take the first, some take the last. An attacker can
		/// exploit that disagreement by crafting a message where a filter or an authentication mechanism such as DKIM validates
		/// one instance while the mail client displays another.
		/// </remarks>
		RepeatedDate                                = 12,

		/// <summary>
		/// The message contained more than one From header field.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification permits at most one From header field. When more than one is present,
		/// agents disagree about which instance is authoritative: some take the first, some take the last. An attacker can
		/// exploit that disagreement by crafting a message where a filter or an authentication mechanism such as DKIM validates
		/// one instance while the mail client displays another.
		/// </remarks>
		RepeatedFrom                                = 13,

		/// <summary>
		/// The message contained more than one Sender header field.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification permits at most one Sender header field. When more than one is present,
		/// agents disagree about which instance is authoritative: some take the first, some take the last. An attacker can
		/// exploit that disagreement by crafting a message where a filter or an authentication mechanism such as DKIM validates
		/// one instance while the mail client displays another.
		/// </remarks>
		RepeatedSender                              = 14,

		/// <summary>
		/// The message contained more than one Reply-To header field.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification permits at most one Reply-To header field. When more than one is present,
		/// agents disagree about which instance is authoritative: some take the first, some take the last. An attacker can
		/// exploit that disagreement by crafting a message where a filter or an authentication mechanism such as DKIM validates
		/// one instance while the mail client displays another.
		/// </remarks>
		RepeatedReplyTo                             = 15,

		/// <summary>
		/// The message contained more than one To header field.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification permits at most one To header field. When more than one is present, agents
		/// disagree about which instance is authoritative: some take the first, some take the last. A message filter may
		/// therefore evaluate one instance while the mail client displays another.
		/// </remarks>
		RepeatedTo                                  = 16,

		/// <summary>
		/// The message contained more than one Cc header field.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification permits at most one Cc header field. When more than one is present, agents
		/// disagree about which instance is authoritative: some take the first, some take the last. A message filter may
		/// therefore evaluate one instance while the mail client displays another.
		/// </remarks>
		RepeatedCc                                  = 17,

		/// <summary>
		/// The message contained more than one Bcc header field.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification permits at most one Bcc header field. When more than one is present,
		/// agents disagree about which instance is authoritative: some take the first, some take the last. A message filter may
		/// therefore evaluate one instance while the mail client displays another.
		/// </remarks>
		RepeatedBcc                                 = 18,

		/// <summary>
		/// The message contained more than one Message-Id header field.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification permits at most one Message-Id header field. When more than one is
		/// present, agents disagree about which instance is authoritative: some take the first, some take the last. An attacker
		/// can exploit that disagreement by crafting a message where a filter or an authentication mechanism such as DKIM
		/// validates one instance while the mail client displays another.
		/// </remarks>
		RepeatedMessageId                           = 19,

		/// <summary>
		/// The message contained more than one In-Reply-To header field.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification permits at most one In-Reply-To header field. When more than one is
		/// present, agents disagree about which instance is authoritative, which may cause the message to be threaded
		/// inconsistently between mail clients.
		/// </remarks>
		RepeatedInReplyTo                           = 20,

		/// <summary>
		/// The message contained more than one References header field.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification permits at most one References header field. When more than one is
		/// present, agents disagree about which instance is authoritative, which may cause the message to be threaded
		/// inconsistently between mail clients.
		/// </remarks>
		RepeatedReferences                          = 21,

		/// <summary>
		/// The message contained more than one Subject header field.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification permits at most one Subject header field. When more than one is present,
		/// agents disagree about which instance is authoritative: some take the first, some take the last. A message filter may
		/// therefore evaluate one instance while the mail client displays another.
		/// </remarks>
		RepeatedSubject                             = 22,

		/// <summary>
		/// The message contained more than one Return-Path header field.
		/// </summary>
		/// <remarks>
		/// Legitimate messages can contain more than one Return-Path header field, but it is more often an error. All but the
		/// topmost instance should be disregarded, because the topmost was added nearest to the mailbox that received the
		/// message.
		/// </remarks>
		RepeatedReturnPath                          = 23,

		/// <summary>
		/// A block of resent header fields contained more than one Resent-Date header field.
		/// </summary>
		/// <remarks>
		/// <para>The Internet Message Format specification permits at most one Resent-Date header field per block of resent
		/// header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the
		/// message.</para>
		/// <para>Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any
		/// other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly
		/// informational and must not be used when processing replies, so the practical consequences are limited to how the
		/// resending history is displayed.</para>
		/// </remarks>
		RepeatedResentDate                          = 24,

		/// <summary>
		/// A block of resent header fields contained more than one Resent-From header field.
		/// </summary>
		/// <remarks>
		/// <para>The Internet Message Format specification permits at most one Resent-From header field per block of resent
		/// header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the
		/// message.</para>
		/// <para>Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any
		/// other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly
		/// informational and must not be used when processing replies, so the practical consequences are limited to how the
		/// resending history is displayed.</para>
		/// </remarks>
		RepeatedResentFrom                          = 25,

		/// <summary>
		/// A block of resent header fields contained more than one Resent-Sender header field.
		/// </summary>
		/// <remarks>
		/// <para>The Internet Message Format specification permits at most one Resent-Sender header field per block of resent
		/// header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the
		/// message.</para>
		/// <para>Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any
		/// other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly
		/// informational and must not be used when processing replies, so the practical consequences are limited to how the
		/// resending history is displayed.</para>
		/// </remarks>
		RepeatedResentSender                        = 26,

		/// <summary>
		/// A block of resent header fields contained more than one Resent-To header field.
		/// </summary>
		/// <remarks>
		/// <para>The Internet Message Format specification permits at most one Resent-To header field per block of resent
		/// header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the
		/// message.</para>
		/// <para>Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any
		/// other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly
		/// informational and must not be used when processing replies, so the practical consequences are limited to how the
		/// resending history is displayed.</para>
		/// </remarks>
		RepeatedResentTo                            = 27,

		/// <summary>
		/// A block of resent header fields contained more than one Resent-Cc header field.
		/// </summary>
		/// <remarks>
		/// <para>The Internet Message Format specification permits at most one Resent-Cc header field per block of resent
		/// header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the
		/// message.</para>
		/// <para>Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any
		/// other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly
		/// informational and must not be used when processing replies, so the practical consequences are limited to how the
		/// resending history is displayed.</para>
		/// </remarks>
		RepeatedResentCc                            = 28,

		/// <summary>
		/// A block of resent header fields contained more than one Resent-Bcc header field.
		/// </summary>
		/// <remarks>
		/// <para>The Internet Message Format specification permits at most one Resent-Bcc header field per block of resent
		/// header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the
		/// message.</para>
		/// <para>Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any
		/// other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly
		/// informational and must not be used when processing replies, so the practical consequences are limited to how the
		/// resending history is displayed.</para>
		/// </remarks>
		RepeatedResentBcc                           = 29,

		/// <summary>
		/// A block of resent header fields contained more than one Resent-Message-Id header field.
		/// </summary>
		/// <remarks>
		/// <para>The Internet Message Format specification permits at most one Resent-Message-Id header field per block of
		/// resent header fields, where a block is a contiguous run of resent fields corresponding to a single resending of the
		/// message.</para>
		/// <para>Because the specification provides no way to delimit adjacent blocks, two blocks that are not separated by any
		/// other header field cannot be told apart and are reported as a repeated field. Resent header fields are strictly
		/// informational and must not be used when processing replies, so the practical consequences are limited to how the
		/// resending history is displayed.</para>
		/// </remarks>
		RepeatedResentMessageId                     = 30,

		#endregion

		#region Content Headers

		/// <summary>
		/// A Content-Type header value was not valid.
		/// </summary>
		/// <remarks>
		/// <para>This indicates that the Content-Type header was not properly formatted and could not be parsed.
		/// Since MIME parsers rely on the Content-Type header to decide how to interpret the content of a MIME
		/// part, an invalid Content-Type header can lead to ambiguity and inconsistent behavior among different
		/// MIME parser implementations.</para>
		/// </remarks>
		InvalidContentType                          = 31,

		/// <summary>
		/// A Content-Transfer-Encoding header value was not valid.
		/// </summary>
		/// <remarks>
		/// This indicates that the Content-Transfer-Encoding header did not contain a valid value and could not be parsed.
		/// </remarks>
		InvalidContentTransferEncoding              = 32,

		/// <summary>
		/// A Content-Transfer-Encoding header for a message/rfc822 part contained an illegal value.
		/// </summary>
		/// <remarks>
		/// <para>The MIME specifications do not allow message/rfc822 Content-Transfer-Encoding headers to specify
		/// any encoding that transforms the content in any way (such as <c>quoted-printable</c> or <c>base64</c>).</para>
		/// <note type="note">The only permissible Content-Transfer-Encoding values for a message/rfc822 part are
		/// <c>7bit</c>, <c>8bit</c>, and <c>binary</c>.</note>
		/// </remarks>
		IllegalMessageRfc822ContentTransferEncoding = 33,

		/// <summary>
		/// A Content-Transfer-Encoding header for a multipart contained an illegal value.
		/// </summary>
		/// <remarks>
		/// <para>The MIME specifications do not allow multipart Content-Transfer-Encoding headers to specify
		/// any encoding that transforms the content in any way (such as <c>quoted-printable</c> or <c>base64</c>).</para>
		/// <note type="note">The only permissible Content-Transfer-Encoding values for a multipart are
		/// <c>7bit</c>, <c>8bit</c>, and <c>binary</c>.</note>
		/// </remarks>
		IllegalMultipartContentTransferEncoding     = 34,

		/// <summary>
		/// A boundary parameter was missing from a multipart Content-Type header.
		/// </summary>
		/// <remarks>
		/// The MIME specifications require that each multipart Content-Type header include a boundary parameter.
		/// A multipart that does not define a boundary can lead to ambiguity and inconsistent behavior among
		/// different MIME parser implementations.
		/// </remarks>
		MissingMultipartBoundaryParameter           = 35,

		/// <summary>
		/// A boundary parameter in a multipart Content-Type header was not valid.
		/// </summary>
		/// <remarks>
		/// A boundary parameter in a multipart Content-Type header must be a valid boundary string as defined by
		/// the MIME specifications. Invalid boundary parameters can lead to ambiguity and inconsistent behavior
		/// among different MIME parser implementations.
		/// </remarks>
		InvalidMultipartBoundaryParameter           = 36,

		#endregion

		#region Address Syntax

		/// <summary>
		/// An address contained more angle brackets than the one pair that delimits an angle-addr.
		/// </summary>
		/// <remarks>
		/// Section 7.1.2 of rfc7103 describes address values such as <c>&lt;&lt;user@example.com&gt;&gt;</c> and notes that
		/// they can safely be interpreted as the same address with a single pair of brackets. Unlike an unbalanced bracket,
		/// a repeated one leaves no doubt about where the address begins and ends, so implementations that discard the
		/// extras all arrive at the same mailbox. It is still a departure from the <c>angle-addr</c> production, and usually
		/// indicates a mailer that has wrapped an address which was already wrapped.
		/// </remarks>
		ExcessiveAngleBracketsInAddress             = 37,

		/// <summary>
		/// An address had an opening angle bracket without a closing one, or a closing bracket without an opening one.
		/// </summary>
		/// <remarks>
		/// Section 7.1.3 of rfc7103 describes address values such as <c>Name &lt;user@example.com</c> and
		/// <c>user@example.org&gt;</c>. Recovering from an unbalanced bracket requires guessing where the address was meant to
		/// end, and parsers that guess differently will extract different addresses.
		/// </remarks>
		UnbalancedAngleBracketsInAddress            = 38,

		/// <summary>
		/// An address contained a quoted-string that was never closed.
		/// </summary>
		/// <remarks>
		/// Section 7.1.6 of rfc7103 describes address values such as <c>"Unterminated &lt;user@example.com&gt;</c>. An unclosed
		/// quote absorbs everything that follows it. Whether an address survives depends on what gets absorbed: a quote
		/// followed by an angle-addr still yields that mailbox, but one followed only by an addr-spec yields nothing at
		/// all. No error is raised either way, so a header that contributes fewer recipients than its author wrote is
		/// indistinguishable from one that parsed cleanly.
		/// </remarks>
		UnbalancedQuotesInAddress                   = 39,

		/// <summary>
		/// An address contained an unbalanced parenthesis in a comment.
		/// </summary>
		/// <remarks>
		/// Section 7.1.4 of rfc7103 describes address values such as <c>Name (unbalanced &lt;user@example.com&gt;</c>. As with
		/// an unclosed quote, an unclosed comment consumes the remainder of the header value, so any addresses that follow
		/// it are lost rather than merely misparsed: text inside a comment carries no meaning, so there is nothing left for
		/// a parser to recover. No error is raised, so the message appears intact while carrying fewer recipients than its
		/// author wrote.
		/// </remarks>
		UnbalancedParenthesesInAddress              = 40,

		/// <summary>
		/// The display-name of an address contained a special character that should have been quoted.
		/// </summary>
		/// <remarks>
		/// An unquoted display-name may only contain atoms, so values such as <c>Doe, John &lt;jdoe@example.com&gt;</c> and
		/// <c>user@example.com &lt;user@example.com&gt;</c> are not valid. The comma case is the most damaging, because a parser
		/// that does not special-case it will split the one address into two.
		/// </remarks>
		UnquotedDisplayName                         = 41,

		/// <summary>
		/// The display-name of a mailbox was itself shaped like an address.
		/// </summary>
		/// <remarks>
		/// A display-name such as the one in <c>"admin@example.com" &lt;attacker@example.org&gt;</c> is legal, but software that
		/// shows the display-name in place of the address will present a mailbox that will not receive the reply. The address
		/// that rfc5322 defines as authoritative is the one inside the angle brackets.
		/// </remarks>
		AddressInDisplayName                        = 42,

		/// <summary>
		/// The display-name of an address group was itself shaped like an address.
		/// </summary>
		/// <remarks>
		/// A group name such as the one in <c>"admin@example.com": attacker@example.org;</c> is legal, but software that shows
		/// the group name in place of its members will present a mailbox that is not in the group. A group name is a label, not
		/// a recipient.
		/// </remarks>
		AddressInGroupDisplayName                   = 43,

		/// <summary>
		/// The local-part of an address was not a valid dot-atom or quoted-string.
		/// </summary>
		/// <remarks>
		/// <para>A dot-atom may not contain two consecutive dots or end with a dot, so local-parts such as
		/// <c>first..last</c> and <c>first.</c> are not valid.</para>
		/// <para>This is also reported when an unquoted special appears inside the local-part, as in
		/// <c>a[b@example.com</c> or <c>a b@example.com</c>. Section 3.4.1 of rfc5322 admits such characters
		/// only inside a quoted-string, so the local-part ends at the offending character and the rest of
		/// the address is left with no production that can consume it.</para>
		/// <para>Receiving systems differ over whether to reject such an address, strip the offending
		/// characters, or pass the local-part through verbatim.</para>
		/// </remarks>
		InvalidLocalPart                            = 44,

		/// <summary>
		/// Two addresses in an address list were not separated by a comma.
		/// </summary>
		/// <remarks>
		/// Section 7.1.5 of rfc7103 describes address lists such as <c>a@example.com b@example.com</c>. A parser must guess
		/// whether this is two addresses or one address with a malformed display-name, and the two readings produce different
		/// sets of recipients.
		/// </remarks>
		MissingAddressSeparator                     = 45,

		/// <summary>
		/// An address list contained two addresses whose boundary was ambiguous.
		/// </summary>
		/// <remarks>
		/// A list such as <c>&lt;attacker@example.org&gt; &lt;admin@example.com&gt;</c> contains an angle-addr on at least one
		/// side of a missing separator, so a parser that recovers by treating the leading text as a display-name will read a
		/// single mailbox where a parser that recovers by splitting will read two. Unlike an ordinary missing comma, the two
		/// readings do not merely differ in how many addresses they produce, they disagree about which mailbox the address
		/// belongs to.
		/// </remarks>
		AmbiguousMailboxBoundary                    = 46,

		/// <summary>
		/// An address list contained a comma that did not separate two addresses.
		/// </summary>
		/// <remarks>
		/// Section 7.1.5 of rfc7103 describes address lists such as <c>a@example.com,,,b@example.com</c>, as well as lists with
		/// leading or trailing commas. The empty entries are not addresses and are typically ignored, but their presence usually
		/// indicates that the generating software dropped an address it intended to include.
		/// </remarks>
		ExtraneousCommaInAddressList                = 47,

		/// <summary>
		/// An address used the obsolete source route syntax.
		/// </summary>
		/// <remarks>
		/// The <c>obs-route</c> syntax described in section 4.4 of rfc5322, as in
		/// <c>&lt;@a.example,@b.example:user@example.com&gt;</c>, must not be generated by conforming software. The route itself
		/// is meant to be ignored, but software that does not recognize the syntax may mistake the first domain in the route for
		/// the address domain.
		/// </remarks>
		ObsoleteRouteAddress                        = 48,

		/// <summary>
		/// An address consisted of a local-part with no domain.
		/// </summary>
		/// <remarks>
		/// Section 7.1.7 of rfc7103 describes "naked" local-parts such as <c>username</c>. Such an address is only meaningful
		/// relative to some implied domain, so different systems will complete it differently, or not at all.
		/// </remarks>
		AddressWithoutDomain                        = 49,

		/// <summary>
		/// The domain of an address used the obsolete syntax that allows comments and whitespace between its parts.
		/// </summary>
		/// <remarks>
		/// The <c>obs-domain</c> syntax described in section 4.4 of rfc5322 allows folding whitespace and comments around the
		/// dots of a domain, as in <c>user@example (comment) .com</c>. A conforming domain is a single dot-atom, so software
		/// that does not implement the obsolete grammar will read a different domain than software that does.
		/// </remarks>
		ObsoleteDomainSyntax                        = 50,

		/// <summary>
		/// The domain of an address ended with a dot.
		/// </summary>
		/// <remarks>
		/// A trailing dot, as in <c>user@example.com.</c>, denotes a fully qualified domain in the DNS but is not part of the
		/// domain grammar in rfc5322. Parsers that strip it and parsers that retain it will disagree about whether two
		/// otherwise identical addresses are equal.
		/// </remarks>
		TrailingDotInDomain                         = 51,

		/// <summary>
		/// A domain-literal contained whitespace.
		/// </summary>
		/// <remarks>
		/// The <c>dtext</c> rule in section 3.4.1 of rfc5322 does not permit whitespace inside the brackets of a domain-literal,
		/// as in <c>user@[ 127.0.0.1 ]</c>. Parsers that strip the whitespace and parsers that preserve or reject it will not
		/// agree on the address.
		/// </remarks>
		WhitespaceInDomainLiteral                   = 52,

		/// <summary>
		/// A domain-literal contained a character that the domain-literal syntax does not permit.
		/// </summary>
		/// <remarks>
		/// The <c>dtext</c> rule in section 3.4.1 of rfc5322 excludes <c>[</c>, <c>]</c> and <c>\</c> from the contents of a
		/// domain-literal, as in <c>user@[10.0.0.1[]</c>. Because <c>]</c> is the only thing that can end a domain-literal, an
		/// unescapable bracket leaves no way to tell where the author intended the address to end, and the domain cannot be
		/// recovered from what was written. Control characters and invalid 8-bit bytes inside a domain-literal are reported as
		/// <see cref="ControlCharacterInAddress"/> and <see cref="Invalid8BitAddress"/> instead, and rfc6532 adds the remaining
		/// 8-bit bytes to <c>dtext</c>.
		/// </remarks>
		InvalidCharacterInDomainLiteral             = 53,

		/// <summary>
		/// An address contained 8-bit bytes that were not valid UTF-8.
		/// </summary>
		/// <remarks>
		/// The internationalized address syntax in rfc6532 extends the address grammar to UTF-8 and to nothing else, so 8-bit
		/// bytes that are not valid UTF-8 have no defined interpretation. A parser that falls back to a single-byte charset
		/// will produce a different address than one that rejects the header, which may result in mail being delivered to the
		/// wrong mailbox.
		/// </remarks>
		Invalid8BitAddress                          = 54,

		/// <summary>
		/// An address group was not terminated with a semi-colon.
		/// </summary>
		/// <remarks>
		/// The group syntax in section 3.4 of rfc5322 requires a terminating <c>;</c>, as in <c>Friends: a@example.com;</c>.
		/// Without it, a parser must guess where the group ends, and addresses that follow the group may be absorbed into it.
		/// </remarks>
		MissingGroupTerminator                      = 55,

		/// <summary>
		/// An address did not conform to the address syntax defined by rfc5322.
		/// </summary>
		/// <remarks>
		/// This is the general case, used when an address departs from the grammar in a way that none of the more specific
		/// violations describes. Receiving systems differ widely in how much malformed syntax they will accept and in how they
		/// repair what they accept, so an address that only some implementations can read may resolve to different mailboxes,
		/// or to none at all, depending on which software handles the message.
		/// </remarks>
		NonConformantAddress                        = 56,

		/// <summary>
		/// An address contained a null byte.
		/// </summary>
		/// <remarks>
		/// <para>A <c>NUL</c> byte is not permitted anywhere in a header, but inside an address it is more dangerous than
		/// elsewhere. Software written in or interfacing with C treats <c>NUL</c> as a string terminator, so
		/// <c>us&lt;NUL&gt;er@example.com</c> may be read as the complete address <c>us</c> by one component and as
		/// <c>user@example.com</c> by another.</para>
		/// <para>That disagreement is the whole point of the construct: a filter, an audit log and the delivering agent can
		/// each be made to see a different mailbox from the same header. Note that this is reported in addition to
		/// <see cref="UnexpectedNullBytesInHeader"/>, which identifies only the line that the null byte appeared on.</para>
		/// </remarks>
		NullByteInAddress                           = 57,

		/// <summary>
		/// The display-name of an address or group contained a null byte.
		/// </summary>
		/// <remarks>
		/// <para>This is reported instead of <see cref="NullByteInAddress"/> when the <c>NUL</c> falls in a display-name
		/// rather than in an addr-spec, because the two call for different handling: a display-name is presentation and can
		/// be discarded without affecting delivery, whereas an addr-spec cannot.</para>
		/// <para>The danger is correspondingly different rather than smaller. The mailbox is unambiguous, but a client that
		/// stops at the <c>NUL</c> shows a different name than one that does not, so <c>Bank&lt;NUL&gt;evil</c> can be made
		/// to read as <c>Bank</c> in the message list and as something else in a filter or an audit log. Note that this is
		/// reported in addition to <see cref="UnexpectedNullBytesInHeader"/>, which identifies only the line that the null
		/// byte appeared on.</para>
		/// </remarks>
		NullByteInDisplayName                       = 58,

		/// <summary>
		/// A line break appeared inside a local-part or domain.
		/// </summary>
		/// <remarks>
		/// <para>Folding whitespace is permitted around the tokens of an address, but the <c>dot-atom-text</c> production in
		/// section 3.2.3 of rfc5322 admits none inside a local-part or domain. A line break within one of those tokens,
		/// as in <c>us&lt;CRLF&gt; er@example.com</c>, cannot be produced by a conforming mailer.</para>
		/// <para>It is most often seen when an application has concatenated unvalidated input into a header, which is the
		/// header injection technique described in section 5 of rfc5321: the attacker supplies a line break in the hope
		/// that some component in the chain will treat what follows as a new header or a new command. Even where that
		/// fails, implementations differ on whether to unfold, reject or truncate the address, so the recipient that is
		/// finally used may not be the one an auditor sees.</para>
		/// <note type="note">A line break inside a <i>quoted</i> local-part is also reported, even though section 3.2.4 of
		/// rfc5322 permits folding whitespace inside a quoted-string. A quoted local-part is rare enough that
		/// implementations mishandle it in practice, so the divergence in the recipient finally used is real regardless of
		/// what the grammar allows.</note>
		/// </remarks>
		LineBreakInAddress                          = 59,

		/// <summary>
		/// An address contained a control character.
		/// </summary>
		/// <remarks>
		/// <para>The <c>atom</c>, <c>quoted-string</c> and <c>domain-literal</c> productions in rfc5322 are all built from
		/// printable characters and whitespace, so a control character such as <c>ESC</c> or <c>DEL</c> can only have been
		/// introduced deliberately or by a mangled encoding.</para>
		/// <para>Control characters are stripped by some implementations and preserved by others, so the address may name a
		/// different mailbox depending on which software resolves it, and an escape sequence that survives into a log or a
		/// terminal-based mail client may be interpreted there rather than displayed. Null bytes and line breaks are
		/// reported separately as <see cref="NullByteInAddress"/> and <see cref="LineBreakInAddress"/>.</para>
		/// </remarks>
		ControlCharacterInAddress                   = 60,

		/// <summary>
		/// The local-part of an address contained an ISO-2022 shift or escape sequence.
		/// </summary>
		/// <remarks>
		/// <para>ISO-2022-JP and its relatives switch between character sets using escape sequences such as
		/// <c>ESC $ B</c>, and, in the Korean and Chinese variants, using the shift-out and shift-in control characters.
		/// Japanese mailers have historically used these inside the <c>local-part</c> of an <c>addr-spec</c> in order to
		/// carry Japanese text in a mailbox name, a practice that predates and is entirely separate from the
		/// internationalized address syntax defined by rfc6532.</para>
		/// <para>This is reported instead of <see cref="ControlCharacterInAddress"/> because the two call for different
		/// handling. An arbitrary control character in an address is either damage or an injection attempt, whereas a
		/// well-formed ISO-2022 sequence in a local-part is a deliberate legacy convention that a receiving system may wish
		/// to decode rather than strip. It remains a violation either way: the <c>atom</c> and <c>quoted-string</c>
		/// productions in rfc5322 are built from printable characters and whitespace, so implementations that strip the
		/// escapes and implementations that preserve them will not agree on which mailbox the address names.</para>
		/// <para>An escape is only recognized as a designation sequence when it is followed by at least one byte in the
		/// <c>0x20..0x2f</c> range and then a final byte in the <c>0x30..0x7e</c> range. An escape that is merely followed
		/// by an ordinary letter is reported as <see cref="ControlCharacterInAddress"/>, as is any ISO-2022 sequence that
		/// appears somewhere other than a <c>local-part</c>.</para>
		/// <para>Recognizing a sequence does not change how the address is parsed. The <c>(</c> of an unquoted
		/// <c>ESC ( B</c> still opens a comment, and is still reported as
		/// <see cref="UnbalancedParenthesesInAddress"/> when nothing closes it, because treating <c>(</c> as ordinary text
		/// whenever an escape happened to precede it would let a genuinely unterminated comment go unreported. Mailers
		/// that use this convention quote the <c>local-part</c> in any case, since that is what allows a <c>\</c> or a
		/// <c>"</c> occurring as the second byte of a JIS X 0208 pair to be written as a <c>quoted-pair</c>.</para>
		/// </remarks>
		Iso2022SequenceInLocalPart                  = 61,

		/// <summary>
		/// An address group had an empty name.
		/// </summary>
		/// <remarks>
		/// The group syntax in section 3.4 of rfc5322 is <c>display-name ":" [group-list] ";"</c>, and a
		/// <c>display-name</c> is a <c>phrase</c>, which requires at least one word. A group introduced by a bare <c>:</c>,
		/// as in <c>To: :;</c>, therefore has no name for a client to display, and parsers disagree over whether to treat
		/// the colon as a group at all or as a stray character in an ordinary address.
		/// </remarks>
		EmptyGroupName                              = 62,

		#endregion

		#region Body Structure

		/// <summary>
		/// An empty line separating the headers from the body was missing.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specifications require that an empty line separate the headers from
		/// the body of a message. This empty line serves as a clear delimiter between the headers and the
		/// body, allowing MIME parsers to correctly identify where the headers end and the body begins.
		/// A missing body separator can lead to ambiguity when parsing the message.
		/// </remarks>
		MissingBodySeparator                        = 63,

		/// <summary>
		/// A multipart boundary was missing.
		/// </summary>
		/// <remarks>
		/// When a multipart does not contain any boundary markers within its content, it can lead to ambiguity
		/// and inconsistent behavior among different MIME parser implementations which may opt to treat the content
		/// as a single part rather than a multipart message.
		/// </remarks>
		MissingMultipartBoundary                    = 64,

		#endregion

		#region Base64 Encoding

		/// <summary>
		/// The base64 encoded content of a MIME part ended with an incomplete quantum.
		/// </summary>
		/// <remarks>
		/// The MIME specifications require base64 encoded content be a multiple of 4 bytes (a "quantum") in length. An
		/// incomplete quantum at the end of the content suggests that the base64 encoded content was either truncated or
		/// otherwise corrupted and can therefore lead to inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		IncompleteBase64Quantum                     = 65,

		/// <summary>
		/// The base64 encoded content of a MIME part contained invalid characters.
		/// </summary>
		/// <remarks>
		/// Invalid characters within base64 content can lead to decoding issues and inconsistent behavior among different MIME
		/// parser implementations which may stop decoding as soon as this scenario is encountered while others may ignore these
		/// characters and continue decoding.
		/// </remarks>
		InvalidBase64Character                      = 66,

		/// <summary>
		/// The base64 encoded content of a MIME part contained invalid padding.
		/// </summary>
		/// <remarks>
		/// Invalid padding within base64 content can lead to decoding issues and inconsistent behavior among different MIME
		/// parser implementations. Some base64 decoders will ignore extraneous '=' padding characters if any are found within
		/// the middle of the base64 encoded block while others will treat decode it as 6 bits of 0's and may stop decoding as
		/// soon as they are encountered.
		/// </remarks>
		InvalidBase64Padding                        = 67,

		/// <summary>
		/// The base64 encoded content of a MIME part contained characters after the padding.
		/// </summary>
		/// <remarks>
		/// Base64 characters found after padding (<c>'='</c>) in a base64 encoded block are not allowed by the MIME specifications
		/// and can lead to inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		Base64CharactersAfterPadding                = 68,

		/// <summary>
		/// The base64 encoded content of a MIME part contained an obsolete comment.
		/// </summary>
		/// <remarks>
		/// RFC 1113 (a Privacy Enhanced Mail specification) allowed for comments delimited by the <c>'*'</c> character in what
		/// later became known as "base64 encoding". This was obsoleted in RFC 1421 (which replaced RFC 1113) and RFC 1341 (the
		/// first MIME specification) explicitly disallowed it, but some mailers may generate such content. Since the vast
		/// majority of MIME base64 decoders do not support comments in base64 content, the presence of such comments can lead
		/// to decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		ObsoleteBase64Comment                       = 69,

		#endregion

		#region Quoted-Printable Encoding

		/// <summary>
		/// The quoted-printable encoded content of a MIME part contained an invalid hex sequence after an '=' character.
		/// </summary>
		/// <remarks>
		/// Incorrect hex-encoded sequences in quoted-printable content can lead to decoding issues and inconsistent behavior among
		/// different MIME parser implementations.
		/// </remarks>
		InvalidQuotedPrintableEncoding              = 70,

		/// <summary>
		/// The quoted-printable encoded content of a MIME part contained an invalid soft-break sequence.
		/// </summary>
		/// <remarks>
		/// A soft line break in quoted-printable content is represented by an equal sign (=) character followed immediately by a
		/// &lt;CR&gt;&lt;LF&gt; sequence. This error indicates that an equal sign was immediately followed by an incomplete
		/// &lt;CR&gt;&lt;LF&gt; sequence which can lead to decoding issues and inconsistent behavior among different MIME parser
		/// implementations.
		/// </remarks>
		InvalidQuotedPrintableSoftBreak             = 71,

		#endregion

		#region UUEncode Encoding

		/// <summary>
		/// The uuencoded content of a MIME part contained non-whitespace content before the begin marker.
		/// </summary>
		/// <remarks>
		/// UUEncoding requires that only lines containing whitespace are allowed before the begin marker. Non-whitespace content
		/// before the begin marker can lead to decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		InvalidUUEncodePretext                      = 72,

		/// <summary>
		/// The uuencoded content of a MIME part had an invalid file mode in the begin marker.
		/// </summary>
		/// <remarks>
		/// The UUEncoding begin marker should contain a file mode that is 3-4 digits long. An invalid file mode can lead to
		/// decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		InvalidUUEncodeFileMode                     = 73,

		/// <summary>
		/// The uuencoded content of a MIME part contained invalid characters or was otherwise malformed.
		/// </summary>
		/// <remarks>
		/// Incorrect line lengths and/or invalid characters in uuencoded content can lead to decoding issues and inconsistent
		/// behavior
		/// among different MIME parser implementations.
		/// </remarks>
		InvalidUUEncodedContent                     = 74,

		/// <summary>
		/// The uuencoded content of a MIME part had an invalid encoded line length.
		/// </summary>
		/// <remarks>
		/// Each line in UUEncoding has a specific length encoded in the first byte of the line. This length must be between 0 and
		/// 45 (inclusive) and is used to determine how many bytes of data are represented by the line. An invalid line length can
		/// lead to decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		InvalidUUEncodedLineLength                  = 75,

		/// <summary>
		/// The uuencoded content of a MIME part contained an incomplete encoded line.
		/// </summary>
		/// <remarks>
		/// Each line in UUEncoding has a specific length encoded in the first byte of the line. Incomplete lines can lead to
		/// decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		IncompleteUUEncodedLine                     = 76,

		/// <summary>
		/// The uuencoded content of a MIME part had extra data beyond the end of a uuencoded line.
		/// </summary>
		/// <remarks>
		/// Each line in UUEncoding has a specific length encoded in the first byte of the line. Extra data beyond the end of the
		/// uuencoded line can lead to decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		InvalidUUEncodedLineExtraData               = 77,

		/// <summary>
		/// The uuencoded content of a MIME part contained non-whitespace content after the end marker.
		/// </summary>
		/// <remarks>
		/// UUEncoding requires that only whitespace is allowed after the end marker. Non-whitespace content after the end marker
		/// can lead to decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		InvalidUUEncodeEndMarker                    = 78,

		/// <summary>
		/// The uuencoded content of a MIME part did not properly end.
		/// </summary>
		/// <remarks>
		/// UUEncoding requires that the encoded content is properly terminated with an end marker. Missing or malformed end markers
		/// can lead to decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		IncompleteUUEncodedContent                  = 79,

		#endregion
	}
}
