//
// SpfStatus.cs
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
	/// The result of an SPF check.
	/// </summary>
	/// <remarks>
	/// <para>The result of an SPF check, as defined by
	/// <a href="https://www.rfc-editor.org/rfc/rfc7208#section-2.6">RFC 7208, Section 2.6</a>.</para>
	/// <para>MimeKit does not perform SPF checks, since they require information from the SMTP session. The result is
	/// supplied by the caller via <see cref="SpfCheckResult"/>.</para>
	/// </remarks>
	/// <seealso cref="SpfCheckResult"/>
	public enum SpfStatus
	{
		/// <summary>
		/// No SPF check was performed or no SPF record was published.
		/// </summary>
		None,

		/// <summary>
		/// The ADMD has explicitly stated that it is not asserting whether the IP address is authorized.
		/// </summary>
		Neutral,

		/// <summary>
		/// The client is authorized to inject mail with the given identity.
		/// </summary>
		Pass,

		/// <summary>
		/// The client is not authorized to use the domain in the given identity.
		/// </summary>
		Fail,

		/// <summary>
		/// The ADMD believes the host is not authorized but is not willing to make a strong policy statement.
		/// </summary>
		SoftFail,

		/// <summary>
		/// The SPF verifier encountered a transient (generally DNS) error while performing the check.
		/// </summary>
		TempError,

		/// <summary>
		/// The domain's published records could not be correctly interpreted.
		/// </summary>
		PermError
	}
}
