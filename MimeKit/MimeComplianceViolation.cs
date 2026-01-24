//
// MimeComplianceViolation.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
	/// <para>An enumeration of potential MIME compliance violations.</para>
	/// </remarks>
	public enum MimeComplianceViolation
	{
		/// <summary>
		/// A bare linefeed character was found in a MIME part or message header.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification requires that all lines be terminated with a
		/// &lt;CR&gt;&lt;LF&gt; sequence. Messages that deviate from this requirement may not be
		/// processed correctly by some mail software.
		/// </remarks>
		BareLinefeedInHeader,

		/// <summary>
		/// A bare linefeed character was found in the body of the message.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification requires that all lines be terminated with a
		/// &lt;CR&gt;&lt;LF&gt; sequence. Messages that deviate from this requirement may not be
		/// processed correctly by some mail software.
		/// </remarks>
		BareLinefeedInBody,

		/// <summary>
		/// A MIME part or message header contained control (or whitespace) characters in the field name.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification requires that all header field names be composed of
		/// printable ASCII characters and must not contain control characters or whitespace characters.
		/// Inclusion of these characters can lead to divergent behavior among various MIME parsers,
		/// resulting in differences in handling.
		/// </remarks>
		InvalidHeader,

		/// <summary>
		/// A MIME part or message header ended prematurely at the end of the stream.
		/// </summary>
		/// <remarks>
		/// This usually indicates that the message was truncated somewhere in transport and may be a sign that
		/// an earlier MIME parser implementation failed to properly handle certain edge cases such as a null
		/// byte in the message header.
		/// </remarks>
		IncompleteHeader,

		/// <summary>
		/// A Content-Type header value was not valid.
		/// </summary>
		/// <remarks>
		/// This indicates that the Content-Type header was not properly formatted and could not be parsed.
		/// </remarks>
		InvalidContentType,

		/// <summary>
		/// A MIME part contained multiple Content-Type headers.
		/// </summary>
		/// <remarks>
		/// The MIME specifications require that each MIME part contain only one Content-Type header.
		/// Multiple Content-Type headers can lead to ambiguity and inconsistent behavior among different
		/// MIME parser implementations.
		/// </remarks>
		MultipleContentTypes,

		/// <summary>
		/// A Content-Transfer-Encoding header value was not valid.
		/// </summary>
		/// <remarks>
		/// This indicates that the Content-Transfer-Encoding header did not contain a valid value and could not be parsed.
		/// </remarks>
		InvalidContentTransferEncoding,

		/// <summary>
		/// A Content-Transfer-Encoding header for a message/rfc822 part contained an illegal value such as "quoted-printable" or "base64".
		/// </summary>
		/// <remarks>
		/// The MIME specifications do not allow message/rfc822 Content-Transfer-Encoding headers to specify any encoding
		/// that transforms the content in any way (such as quoted-printable or base64).
		/// </remarks>
		IllegalMessageRfc822ContentTransferEncoding,

		/// <summary>
		/// A Content-Transfer-Encoding header for a multipart contained an illegal value such as "quoted-printable" or "base64".
		/// </summary>
		/// <remarks>
		/// The MIME specifications do not allow multipart Content-Transfer-Encoding headers to specify any encoding
		/// that transforms the content in any way (such as quoted-printable or base64).
		/// </remarks>
		IllegalMultipartContentTransferEncoding,

		/// <summary>
		/// A MIME part contained multiple Content-Transfer-Encoding headers.
		/// </summary>
		/// <remarks>
		/// The MIME specifications require that each MIME part contain only one Content-Transfer-Encoding header.
		/// Multiple Content-Transfer-Encoding headers can lead to ambiguity and inconsistent behavior among different
		/// MIME parser implementations.
		/// </remarks>
		MultipleContentTransferEncodings,

		/// <summary>
		/// A line was found that was longer than the SMTP limit of 1000 characters.
		/// </summary>
		/// <remarks>
		/// This indicates that a line was longer than the SMTP limit of 1000 characters.
		/// </remarks>
		InvalidWrapping,

		/// <summary>
		/// An empty line separating the headers from the body was missing.
		/// </summary>
		/// <remarks>
		/// A missing body separator can lead to ambiguity when parsing the message, as it becomes unclear
		/// where the headers end and the body begins.
		/// </remarks>
		MissingBodySeparator,

		/// <summary>
		/// A boundary parameter was missing from a multipart Content-Type header.
		/// </summary>
		/// <remarks>
		/// The MIME specifications require that each multipart Content-Type header include a boundary parameter.
		/// A multipart that does not define a boundary can lead to ambiguity and inconsistent behavior among different
		/// MIME parser implementations.
		/// </remarks>
		MissingMultipartBoundaryParameter,

		/// <summary>
		/// A boundary parameter in a multipart Content-Type header was not valid.
		/// </summary>
		/// <remarks>
		/// A boundary parameter in a multipart Content-Type header must be a valid boundary string as defined by the MIME specifications.
		/// Invalid boundary parameters can lead to ambiguity and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		InvalidMultipartBoundaryParameter,

		/// <summary>
		/// A multipart boundary was missing.
		/// </summary>
		/// <remarks>
		/// When a multipart does not contain any boundary markers within its content, it can lead to ambiguity
		/// and inconsistent behavior among different MIME parser implementations which may opt to treat the content
		/// as a single part rather than a multipart message.
		/// </remarks>
		MissingMultipartBoundary,

		/// <summary>
		/// A MIME part or message header contained 8-bit bytes where only 7-bit bytes were expected.
		/// </summary>
		/// <remarks>
		/// The Internet Message Format specification requires that headers are strictly US-ASCII. Header values that are not
		/// US-ASCII should be encoded using the encoding mechanism described in the MIME specification and/or should be
		/// valid UTF-8 as allowed in the Internationalized Email Headers specification.
		/// </remarks>
		Unexpected8BitBytesInHeaders,

		/// <summary>
		/// A MIME part's body contained 8-bit content where only 7-bit content was expected.
		/// </summary>
		/// <remarks>
		/// This indicates that the Content-Transfer-Encoding header for a MIME part was set to <c>7bit</c> but contained
		/// non-ASCII text (or potentially even binary data).
		/// </remarks>
		Unexpected8BitBytesInBody,

		/// <summary>
		/// A MIME part or message header contained illegal null (<c>0x00</c>) bytes.
		/// </summary>
		/// <remarks>
		/// Null (0x00) bytes in a message header can be used by malicious actors to prevent some MIME parsers, such as those
		/// written in languages like C or C++, from discovering content after the null byte. This technique can be used to
		/// smuggle viruses or other malicious content past content scanners.
		/// </remarks>
		UnexpectedNullBytesInHeader,

		/// <summary>
		/// A MIME part's body contained null (<c>0x00</c>) bytes without specifying a binary transfer encoding.
		/// </summary>
		/// <remarks>
		/// Null (0x00) bytes in a message body can be used by malicious actors to prevent some MIME parsers, such as those
		/// written in languages like C or C++, from discovering content after the null byte. This technique can be used to
		/// smuggle viruses or other malicious content past content scanners.
		/// </remarks>
		UnexpectedNullBytesInBody,

		/// <summary>
		/// The base64 encoded content of a MIME part contained an incomplete quantum on a line.
		/// </summary>
		/// <remarks>
		/// The MIME specifications require that each line of base64 encoded content be a multiple of 4 bytes (a "quantum") in length.
		/// Lines that break a base64 quantum across multiple lines is not allowed by the specifications and can therefore lead to
		/// inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		IncompleteBase64Quantum,

		/// <summary>
		/// The base64 encoded content of a MIME part contained invalid characters.
		/// </summary>
		/// <remarks>
		/// Invalid characters within base64 content can lead to decoding issues and inconsistent behavior among different MIME
		/// parser implementations which may stop decoding as soon as this scenario is encountered while others may ignore these
		/// characters and continue decoding.
		/// </remarks>
		InvalidBase64Character,

		/// <summary>
		/// The base64 encoded content of a MIME part contained invalid padding.
		/// </summary>
		/// <remarks>
		/// Invalid padding within base64 content can lead to decoding issues and inconsistent behavior among different MIME
		/// parser implementations. Some base64 decoders will ignore extraneous '=' padding characters if any are found within
		/// the middle of the base64 encoded block while others will treat decode it as 6 bits of 0's and may stop decoding as
		/// soon as they are encountered.
		/// </remarks>
		InvalidBase64Padding,

		/// <summary>
		/// The base64 encoded content of a MIME part contained characters after the padding.
		/// </summary>
		/// <remarks>
		/// Base64 characters found after padding (<c>'='</c>) in a base64 encoded block are not allowed by the MIME specifications
		/// and can lead to inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		Base64CharactersAfterPadding,

		/// <summary>
		/// The quoted-printable encoded content of a MIME part contained an invalid hex sequence after an '=' character.
		/// </summary>
		/// <remarks>
		/// Incorrect hex-encoded sequences in quoted-printable content can lead to decoding issues and inconsistent behavior among
		/// different MIME parser implementations.
		/// </remarks>
		InvalidQuotedPrintableEncoding,

		/// <summary>
		/// The quoted-printable encoded content of a MIME part contained an invalid soft-break sequence.
		/// </summary>
		/// <remarks>
		/// A soft line break in quoted-printable content is represented by an equal sign (=) character followed immediately by a
		/// &lt;CR&gt;&lt;LF&gt; sequence. This error indicates that an equal sign was immediately followed by an incomplete
		/// &lt;CR&gt;&lt;LF&gt; sequence which can lead to decoding issues and inconsistent behavior among different MIME parser
		/// implementations.
		/// </remarks>
		InvalidQuotedPrintableSoftBreak,

		/// <summary>
		/// The uuencoded content of a MIME part contained non-whitespace content before the begin marker.
		/// </summary>
		/// <remarks>
		/// UUEncoding requires that only lines containing whitespace are allowed before the begin marker. Non-whitespace content
		/// before the begin marker can lead to decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		InvalidUUEncodePretext,

		/// <summary>
		/// The uuencoded content of a MIME part had an invalid file mode in the begin marker.
		/// </summary>
		/// <remarks>
		/// The UUEncoding begin marker should contain a file mode that is 3-4 digits long. An invalid file mode can lead to
		/// decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		InvalidUUEncodeFileMode,

		/// <summary>
		/// The uuencoded content of a MIME part contained invalid characters or was otherwise malformed.
		/// </summary>
		/// <remarks>
		/// Incorrect line lengths and/or invalid characters in uuencoded content can lead to decoding issues and inconsistent behavior
		/// among different MIME parser implementations.
		/// </remarks>
		InvalidUUEncodedContent,

		/// <summary>
		/// The uuencoded content of a MIME part had extra data beyond the end of a uuencoded line.
		/// </summary>
		/// <remarks>
		/// Each line in UUEncoding has a specific length encoded in the first byte of the line. Extra data beyond the end of the
		/// uuencoded line can lead to decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		InvalidUUEncodedLineLength,

		/// <summary>
		/// The uuencoded content of a MIME part contained non-whitespace content after the end marker.
		/// </summary>
		/// <remarks>
		/// UUEncoding requires that only whitespace is allowed after the end marker. Non-whitespace content after the end marker
		/// can lead to decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		InvalidUUEncodeEndMarker,

		/// <summary>
		/// The uuencoded content of a MIME part did not properly end.
		/// </summary>
		/// <remarks>
		/// UUEncoding requires that the encoded content is properly terminated with an end marker. Missing or malformed end markers
		/// can lead to decoding issues and inconsistent behavior among different MIME parser implementations.
		/// </remarks>
		IncompleteUUEncodedContent,
	}
}
