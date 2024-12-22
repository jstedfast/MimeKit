//
// MimeComplianceStatus.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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

namespace MimeKit
{
	/// <summary>
	/// A bitfield of potential MIME compliance issues.
	/// </summary>
	/// <remarks>
	/// A bitfield of potential MIME compliance issues.
	/// </remarks>
	[Flags]
	public enum MimeComplianceStatus {
		/// <summary>
		/// The MIME is compliant.
		/// </summary>
		Compliant                           = 0,

		/// <summary>
		/// The header was not of the correct form.
		/// </summary>
		InvalidHeader                       = 1 << 0,

		/// <summary>
		/// The header ended prematurely at the end of the stream.
		/// </summary>
		IncompleteHeader                    = 1 << 1,

		/// <summary>
		/// The Content-Transfer-Encoding header value was not valid.
		/// </summary>
		InvalidContentTransferEncoding      = 1 << 2,

		/// <summary>
		/// The Content-Type header value was not valid.
		/// </summary>
		InvalidContentType                  = 1 << 3,

		/// <summary>
		/// The MIME-Version header value was not valid.
		/// </summary>
		InvalidMimeVersion                  = 1 << 4,

		/// <summary>
		/// A line was found that was longer than the SMTP limit of 1000 characters.
		/// </summary>
		InvalidWrapping                     = 1 << 5,

		/// <summary>
		/// An empty line separating the headers from the body was missing.
		/// </summary>
		MissingBodySeparator                = 1 << 6,

		/// <summary>
		/// The MIME-Version header is missing.
		/// </summary>
		MissingMimeVersion                  = 1 << 7,

		/// <summary>
		/// The boundary parameter is missing from a multipart Content-Type header.
		/// </summary>
		MissingMultipartBoundaryParameter   = 1 << 8,

		/// <summary>
		/// A multipart boundary was missing.
		/// </summary>
		MissingMultipartBoundary            = 1 << 9,

		/// <summary>
		/// A MIME part contained multiple Content-Transfer-Encoding headers.
		/// </summary>
		DuplicateContentTransferEncoding    = 1 << 10,

		/// <summary>
		/// A MIME part contained multiple Content-Type headers.
		/// </summary>
		DuplicateContentType                = 1 << 11,

#if false
		/// <summary>
		/// A line was found in a MIME part body content that was linefeed terminated instead of carriage return &amp; linefeed terminated.
		/// </summary>
		BareLinefeedInBody,

		/// <summary>
		/// An external body was specified with invalid syntax.
		/// </summary>
		InvalidExternalBody,

		/// <summary>
		/// A line was found in a MIME part header that was linefeed terminated instead of carriage return &amp; linefeed terminated.
		/// </summary>
		BareLinefeedInHeader,

		/// <summary>
		/// Unexpected binary content was found in MIME part body content.
		/// </summary>
		UnexpectedBinaryContent,
#endif
	}
}
