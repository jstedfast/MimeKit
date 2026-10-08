//
// DmarcErrors.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// The problems encountered during DMARC validation.
	/// </summary>
	/// <remarks>
	/// The problems encountered during DMARC validation, which explain the
	/// <see cref="DmarcValidationResult.Status"/> of the result.
	/// </remarks>
	/// <seealso cref="DmarcValidationResult.Errors"/>
	[Flags]
	public enum DmarcErrors
	{
		/// <summary>
		/// No problems were encountered.
		/// </summary>
		None = 0,

		/// <summary>
		/// The message does not have a From header.
		/// </summary>
		NoFromHeader = 1 << 0,

		/// <summary>
		/// The message has more than one From header.
		/// </summary>
		MultipleFromHeaders = 1 << 1,

		/// <summary>
		/// The From header does not contain any mailbox with a domain.
		/// </summary>
		NoAuthorDomain = 1 << 2,

		/// <summary>
		/// The From header contains a domain that is not a valid DNS domain name (such as a domain literal).
		/// </summary>
		InvalidAuthorDomain = 1 << 3,

		/// <summary>
		/// The From header contains more Author Domains than <see cref="DmarcVerifier.MaxAuthorDomains"/>.
		/// </summary>
		MultipleAuthorDomains = 1 << 4,

		/// <summary>
		/// Multiple DMARC Policy Records were published for a single name, so they were all discarded.
		/// </summary>
		MultipleRecords = 1 << 5,

		/// <summary>
		/// The applicable DMARC Policy Record has a missing or invalid <c>p</c> tag, or an invalid <c>sp</c> or <c>np</c> tag.
		/// </summary>
		/// <remarks>
		/// If the record has a valid <c>rua</c> tag, it is treated as a record with <c>p=none</c>. Otherwise,
		/// it is ignored and the result is <see cref="DmarcStatus.None"/>
		/// (<a href="https://www.rfc-editor.org/rfc/rfc9989#section-4.10.1">RFC 9989, Section 4.10.1</a>).
		/// </remarks>
		InvalidPolicyRecord = 1 << 6,

		/// <summary>
		/// A DNS query required for DMARC validation failed with a temporary error.
		/// </summary>
		DnsTemporaryFailure = 1 << 7,

		/// <summary>
		/// No Authenticated Identifier was aligned, and a DKIM signature that would have been aligned had a
		/// temporary error.
		/// </summary>
		DkimTemporaryError = 1 << 8,

		/// <summary>
		/// No Authenticated Identifier was aligned, and an SPF check for a domain that would have been aligned had
		/// a temporary error.
		/// </summary>
		SpfTemporaryError = 1 << 9
	}
}
