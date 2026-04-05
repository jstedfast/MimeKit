//
// TestMimeComplianceLogger.cs
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

using MimeKit;

namespace UnitTests {
	class TestMimeComplianceLogger : IMimeComplianceLogger
	{
		public TestMimeComplianceLogger ()
		{
			Issues = new List<MimeComplianceIssue> ();
		}

		public void Log (MimeComplianceViolation violation, long streamOffset, int lineNumber, int columnNumber = -1)
		{
			Issues.Add (new MimeComplianceIssue (violation, streamOffset, lineNumber, columnNumber));
		}

		public List<MimeComplianceIssue> Issues { get; private set; }

#if false
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
			case MimeComplianceViolation.IncompleteBase64LineQuantum:
				return "The base64 encoded content of a MIME part contained an incomplete quantum on a line.";
			case MimeComplianceViolation.IncompleteBase64EndQuantum:
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
				return "The uuencoded content of a MIME part had extra data beyond the end of a uuencoded line.";
			case MimeComplianceViolation.InvalidUUEncodeEndMarker:
				return "The uuencoded content of a MIME part contained non-whitespace content after the end marker.";
			case MimeComplianceViolation.IncompleteUUEncodedContent:
				return "The uuencoded content of a MIME part did not properly end.";
			default:
				return string.Empty;
			}
		}

		public static string? GetRemarks (MimeComplianceViolation violation)
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
				return "Null (0x00) bytes in a message body can be used by malicious actors to prevent some MIME parsers, such as those written in languages like C or C++which tend to use the null byte to mark the end of a buffer, from discovering content after the null byte. This technique can be used to smuggle viruses or other malicious content past content scanners.";
			case MimeComplianceViolation.IncompleteBase64LineQuantum:
				return "While the MIME specifications do not require each line of base64 encoded content be a multiple of 4 bytes (a \"quantum\") in length, it is not a common scenario and may break some decoders which expect lines to contain complete quantums.";
			case MimeComplianceViolation.IncompleteBase64EndQuantum:
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
				return "Each line in UUEncoding has a specific length encoded in the first byte of the line. Extra data beyond the end of the uuencoded line can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.InvalidUUEncodeEndMarker:
				return "UUEncoding requires that only whitespace is allowed after the end marker. Non-whitespace content after the end marker can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			case MimeComplianceViolation.IncompleteUUEncodedContent:
				return "UUEncoding requires that the encoded content is properly terminated with an end marker. Missing or malformed end markers can lead to decoding issues and inconsistent behavior among different MIME parser implementations.";
			default:
				return null;
			}
		}
#endif
	}
}
