//
// DmarcPublicSuffixDomain.cs
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
	/// The value of the <c>psd</c> tag of a DMARC Policy Record.
	/// </summary>
	/// <remarks>
	/// <para>Indicates whether the domain that published a DMARC Policy Record is a Public Suffix Domain (PSD).
	/// This is used by the DNS Tree Walk to determine the Organizational Domain.</para>
	/// <para>See <a href="https://www.rfc-editor.org/rfc/rfc9989#section-4.7">RFC 9989, Section 4.7</a>
	/// for details.</para>
	/// </remarks>
	/// <seealso cref="DmarcRecord"/>
	public enum DmarcPublicSuffixDomain
	{
		/// <summary>
		/// The domain is not a PSD and may or may not be an Organizational Domain (<c>psd=u</c>). This is the
		/// default.
		/// </summary>
		Unspecified,

		/// <summary>
		/// The domain is a PSD (<c>psd=y</c>).
		/// </summary>
		Yes,

		/// <summary>
		/// The domain is not a PSD, but it is the Organizational Domain for itself and its subdomains
		/// (<c>psd=n</c>).
		/// </summary>
		No
	}
}
