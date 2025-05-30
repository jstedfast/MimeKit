//
// MimeComplianceStatus.cs
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

using System;

namespace MimeKit {
	/// <summary>
	/// A bitfield of potential MIME compliance issues.
	/// </summary>
	/// <remarks>
	/// <para>A bitfield of potential MIME compliance issues.</para>
	/// </remarks>
	[Flags]
	public enum MimeComplianceStatus
	{
		/// <summary>
		/// The MIME is compliant.
		/// </summary>
		/// <remarks>
		/// No known issues were detected while parsing the message or MIME entity.
		/// </remarks>
		Compliant = 0,

		/// <summary>
		/// A bare linefeed character was found in a MIME part or message header.
		/// </summary>
		BareLinefeedInHeader = 1 << 0,

		/// <summary>
		/// A bare linefeed character was found in the body of the message.
		/// </summary>
		BareLinefeedInBody = 1 << 1,

		/// <summary>
		/// A MIME part or message header was improperly formatted.
		/// </summary>
		InvalidHeader = 1 << 2,

		/// <summary>
		/// A MIME part or message header ended prematurely at the end of the stream.
		/// </summary>
		IncompleteHeader = 1 << 3,

		/// <summary>
		/// A Content-Type header value was not valid.
		/// </summary>
		InvalidContentType = 1 << 4,

		/// <summary>
		/// A MIME part contained multiple Content-Type headers.
		/// </summary>
		MultipleContentTypes = 1 << 5,

		/// <summary>
		/// A Content-Transfer-Encoding header value was not valid.
		/// </summary>
		InvalidContentTransferEncoding = 1 << 6,

		/// <summary>
		/// A MIME part contained multiple Content-Transfer-Encoding headers.
		/// </summary>
		MultipleContentTransferEncodings = 1 << 7,

		/// <summary>
		/// A line was found that was longer than the SMTP limit of 1000 characters.
		/// </summary>
		InvalidWrapping = 1 << 8,

		/// <summary>
		/// An empty line separating the headers from the body was missing.
		/// </summary>
		MissingBodySeparator = 1 << 9,

		/// <summary>
		/// A boundary parameter was missing from a multipart Content-Type header.
		/// </summary>
		MissingMultipartBoundaryParameter = 1 << 10,

		/// <summary>
		/// A multipart boundary was missing.
		/// </summary>
		MissingMultipartBoundary = 1 << 11,

		/// <summary>
		/// A MIME part or message header contained 8-bit bytes where only 7-bit bytes were expected.
		/// </summary>
		Unexpected8BitBytesInHeaders = 1 << 12,

		/// <summary>
		/// A MIME part's body contained 8-bit bytes where only 7-bit bytes were expected.
		/// </summary>
		Unexpected8BitBytesInBody = 1 << 13,

		/// <summary>
		/// A MIME part or message header contained illegal null bytes.
		/// </summary>
		UnexpectedNullBytesInHeader = 1 << 14,

		/// <summary>
		/// A MIME part's body contained null bytes without specifying a binary transfer encoding.
		/// </summary>
		UnexpectedNullBytesInBody = 1 << 15,
	}
}
