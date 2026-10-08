//
// DmarcFailureReportingOptions.cs
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
	/// The DMARC failure reporting options.
	/// </summary>
	/// <remarks>
	/// <para>The conditions under which a Domain Owner requests failure reports, as specified by the <c>fo</c>
	/// tag of a DMARC Policy Record.</para>
	/// <para>See <a href="https://www.rfc-editor.org/rfc/rfc9989#section-4.7">RFC 9989, Section 4.7</a>
	/// for details.</para>
	/// </remarks>
	/// <seealso cref="DmarcRecord"/>
	[Flags]
	public enum DmarcFailureReportingOptions
	{
		/// <summary>
		/// No failure reporting options.
		/// </summary>
		None = 0,

		/// <summary>
		/// Generate a DMARC failure report if all underlying authentication mechanisms fail to produce an aligned
		/// "pass" result (<c>fo=0</c>). This is the default.
		/// </summary>
		AllFail = 1 << 0,

		/// <summary>
		/// Generate a DMARC failure report if any underlying authentication mechanism fails to produce an
		/// aligned "pass" result (<c>fo=1</c>).
		/// </summary>
		AnyFail = 1 << 1,

		/// <summary>
		/// Generate a DKIM failure report if the message had a signature that failed evaluation, regardless of
		/// its alignment (<c>fo=d</c>).
		/// </summary>
		Dkim = 1 << 2,

		/// <summary>
		/// Generate an SPF failure report if the message failed SPF evaluation, regardless of its alignment
		/// (<c>fo=s</c>).
		/// </summary>
		Spf = 1 << 3
	}
}
