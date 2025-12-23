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
		/// printable ASCII characters and must not contain control characters (i.e., characters with
		/// codes 0-31) or whitespace characters (i.e., space and horizontal tab). Inclusion of these
		/// characters can lead to divergent behavior among various MIME parsers, resulting in differences
		/// in handling.
		/// </remarks>
		InvalidHeader,

		/// <summary>
		/// A MIME part or message header ended prematurely at the end of the stream.
		/// </summary>
		IncompleteHeader,

		/// <summary>
		/// A Content-Type header value was not valid.
		/// </summary>
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
		Unexpected8BitBytesInHeaders,

		/// <summary>
		/// A MIME part's body contained 8-bit content where only 7-bit content was expected.
		/// </summary>
		Unexpected8BitBytesInBody,

		/// <summary>
		/// A MIME part or message header contained illegal null (<c>0x00</c>) bytes.
		/// </summary>
		UnexpectedNullBytesInHeader,

		/// <summary>
		/// A MIME part's body contained null (<c>0x00</c>) bytes without specifying a binary transfer encoding.
		/// </summary>
		UnexpectedNullBytesInBody,
	}
}
