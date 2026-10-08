//
// DmarcStatus.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// The result of DMARC validation.
	/// </summary>
	/// <remarks>
	/// <para>The DMARC result codes, as defined by
	/// <a href="https://www.rfc-editor.org/rfc/rfc9989#section-9.2">RFC 9989, Section 9.2</a>.</para>
	/// <para>The exact cause of a <see cref="None"/>, <see cref="TempError"/> or <see cref="PermError"/> result
	/// is reported by <see cref="DmarcValidationResult.Errors"/>.</para>
	/// </remarks>
	/// <seealso cref="DmarcValidationResult"/>
	public enum DmarcStatus
	{
		/// <summary>
		/// No DMARC Policy Record exists for the Author Domain, so the DMARC mechanism does not apply.
		/// </summary>
		None,

		/// <summary>
		/// A DMARC Policy Record exists for the Author Domain, and an Authenticated Identifier with Identifier
		/// Alignment exists.
		/// </summary>
		Pass,

		/// <summary>
		/// A DMARC Policy Record exists for the Author Domain, but no Authenticated Identifier with Identifier
		/// Alignment exists.
		/// </summary>
		Fail,

		/// <summary>
		/// An error that is likely transient in nature, such as a DNS server being temporarily unreachable,
		/// occurred during DMARC evaluation.
		/// </summary>
		TempError,

		/// <summary>
		/// An unrecoverable error occurred during DMARC evaluation, such as a message without exactly one
		/// From header.
		/// </summary>
		PermError
	}
}
