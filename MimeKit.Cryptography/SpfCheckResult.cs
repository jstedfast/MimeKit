//
// SpfCheckResult.cs
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
	/// The result of an SPF check of the MAIL FROM identity.
	/// </summary>
	/// <remarks>
	/// <para>The result of an SPF check, as performed by the SMTP server that received the message.</para>
	/// <para>DMARC only uses the SPF result for the MAIL FROM identity. If the SMTP MAIL command had a null
	/// reverse-path, the domain is the HELO identity
	/// (see <a href="https://www.rfc-editor.org/rfc/rfc9989#section-3.2.4">RFC 9989, Section 3.2.4</a>).</para>
	/// </remarks>
	/// <seealso cref="DmarcVerifier"/>
	public sealed class SpfCheckResult
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="SpfCheckResult"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="SpfCheckResult"/>.
		/// </remarks>
		/// <param name="status">The result of the SPF check.</param>
		/// <param name="domain">The domain that was checked.</param>
		/// <param name="reason">The reason for the result, if available.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="status"/> is not a valid <see cref="SpfStatus"/>.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="domain"/> is <see langword="null"/>.
		/// </exception>
		public SpfCheckResult (SpfStatus status, string domain, string? reason = null)
		{
			if (status < SpfStatus.None || status > SpfStatus.PermError)
				throw new ArgumentOutOfRangeException (nameof (status));

			if (domain is null)
				throw new ArgumentNullException (nameof (domain));

			Status = status;
			Domain = domain;
			Reason = reason;
		}

		/// <summary>
		/// Get the result of the SPF check.
		/// </summary>
		/// <remarks>
		/// Gets the result of the SPF check.
		/// </remarks>
		/// <value>The result of the SPF check.</value>
		public SpfStatus Status {
			get;
		}

		/// <summary>
		/// Get the domain that was checked.
		/// </summary>
		/// <remarks>
		/// Gets the domain of the MAIL FROM identity (or the HELO identity if the reverse-path was null).
		/// </remarks>
		/// <value>The domain.</value>
		public string Domain {
			get;
		}

		/// <summary>
		/// Get the reason for the result.
		/// </summary>
		/// <remarks>
		/// Gets the reason for the result, if available.
		/// </remarks>
		/// <value>The reason or <see langword="null"/>.</value>
		public string? Reason {
			get;
		}
	}
}
